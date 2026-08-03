using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Reward_Flow_v2.Employees;
using RewardFlow.IntegrationTests.Infrastructure;
using Reward_Flow_v2.Employees.Data;
using Reward_Flow_v2.Employees.CreateEmployee;
using Reward_Flow_v2.Employees.BulkInsertEmployees;
using RewardFlow.IntegrationTests.Employees.Common;
using RewardFlow.TestUtilities.DataGenerators;
using RewardFlow.TestUtilities.DataGenerators.Fakers.Employees;
using System.Diagnostics;
using Xunit;
using Xunit.Abstractions;
using Hangfire.Storage.Monitoring;
using Microsoft.Extensions.DependencyInjection;
using RewardFlow.TestUtilities.UtilityClasses;


namespace RewardFlow.IntegrationTests.Employees.BulkOperations.V2;

public class BulkCreateTestsV2(TestWebApplicationFactory factory, ITestOutputHelper _output)
    : BaseEmployeeTestFixture(factory), IAsyncLifetime
{
    private UserClient _userClient;

    public async Task InitializeAsync()
    {
        var user = TestDataGenerator.User.Generate();
        await _dbUtility.InsertAsync(user);
        _userClient = new UserClient(_factory, user);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    /// <summary>
    /// Helper to post the bulk request and extract the returned Batch ID
    /// </summary>
    private async Task<Guid> PostBulkInsertAsync(IEnumerable<Employee> employees)
    {
        var bulkRequest = BulkInsertEmployeeRequest(employees);
        var response = await _userClient.Client.PostAsJsonAsync(EmployeeApiPath.BulkInsertV2, bulkRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);

        // The API returns Results.Accepted(value: batch.Id), which serializes the Guid
        var batchId = await response.Content.ReadFromJsonAsync<Guid>();
        batchId.Should().NotBeEmpty().Should().NotBe(Guid.Empty);

        return batchId;
    }

    private async Task WaitEmployeesProcessing(int expectedEmployeeCount, int waitSeconds)
    {
        var latestEmployeeCount = 0;

        await Waiter.Wait(async () =>
        {
            var employees = await _dbUtility.Query<Employee>()
                .Where(e => e.CreatedBy == _userClient.User.Id)
                .Include(e => e.NameTokens)
                .ToListAsync();

            var currentCount = employees.Count;

            var isDone = currentCount == expectedEmployeeCount;
            var isProcessing = latestEmployeeCount > currentCount;

            latestEmployeeCount = currentCount;

            return new WaiterResult(isDone, isProcessing);
        }, waitSeconds, 5000,"Stable check limit reached. Employee Inserting Background job likely failed or stalled.",50);
    }

    private async Task WaitTokensProcessing(int waitSeconds)
    {
        var lastCompletedCount = 0;

        await Waiter.Wait(async () =>
        {
            var employees = await _dbUtility.Query<Employee>()
                .Where(e => e.CreatedBy == _userClient.User.Id)
                .Include(e => e.NameTokens)
                .ToListAsync();

            // Count employees that has tokens that are fully processed (inserted AND have their tokens generated)
            int completedCount = employees.Count(e =>
                e.NameTokens != null
                && e.NameTokens.Count >=
                (TotalTokensCount(e.Name.Length, 2) + TotalTokensCount(e.Name.Length, 3))*0.7);

            var isProcessing = lastCompletedCount > completedCount;
            lastCompletedCount = completedCount;

            return new WaiterResult(completedCount == employees.Count, isProcessing);
        }, waitSeconds, 20,"Stable check limit reached. Token Background job likely failed or stalled.", 500);
        return;

        int TotalTokensCount(int length, int tokenLength) { return Math.Max(0, length - tokenLength + 1); }
    }

    /// <summary>
    /// Waits for the background jobs to insert employees and generate their tokens.
    /// Employees are inserted first, then tokens are generated.
    /// </summary>
    private async Task WaitForEmployeesAndTokensAsync(int expectedEmployeeCount)
    {
        var overAllStopwatch = Stopwatch.StartNew();
        
        var employeeStopwatch = Stopwatch.StartNew();
        await WaitEmployeesProcessing(expectedEmployeeCount,30);
        employeeStopwatch.Stop();
        
        
        var tokensStopwatch = Stopwatch.StartNew();
        await WaitTokensProcessing(120);
        tokensStopwatch.Stop();
        
        overAllStopwatch.Stop();
        
        _output.WriteLine($"✅ EMPLOYEE INSERTION TIME: Processed {expectedEmployeeCount} employees in {employeeStopwatch.Elapsed.TotalSeconds:F2} seconds.");
        _output.WriteLine($"✅ TOKEN GENERATION TIME: Processed {expectedEmployeeCount} employees in {tokensStopwatch.Elapsed.TotalSeconds:F2} seconds.");
        _output.WriteLine($"✅ OVERALL TIME: Processed {expectedEmployeeCount} employees in {overAllStopwatch.Elapsed.TotalSeconds:F2} seconds.");
    }

    [Fact]
    public async Task BulkInsertEmployees_SpeedTest_MeasureExecutionTime()
    {
        const int employeesCount = 200;

        // Arrange
        var employees = TestDataGenerator.Employee
            .ForProperty(e => e.CreatedBy, _userClient.User.Id)
            .Generate(employeesCount);

        // ACT
        var stopwatch = Stopwatch.StartNew();

        var batchId = await PostBulkInsertAsync(employees);

        await WaitForHangfireJobFromFactoryAsync(batchId.ToString());

        stopwatch.Stop();

        // 🎯 PRINT YOUR CV NUMBER HERE
        // This will output the exact time to your test runner's console/output window
        _output.WriteLine(
            $"✅ SPEED TEST RESULT: Processed {employeesCount} employees in {stopwatch.Elapsed.TotalSeconds:F2} seconds.");
    }

    private async Task WaitForHangfireJobFromFactoryAsync(string batchId)
    {
        var timeout = TimeSpan.FromSeconds(1200);
        var start = DateTime.UtcNow;

        while (DateTime.UtcNow - start < timeout)
        {
            // 🚀 1. GET HANGFIRE DIRECTLY FROM THE FACTORY'S DI CONTAINER
            using var scope = _factory.Services.CreateScope();
            var jobStorage = scope.ServiceProvider.GetRequiredService<Hangfire.JobStorage>();
            var api = jobStorage.GetMonitoringApi();

            // 🚀 2. CHECK THE MOST RECENT SUCCEEDED JOBS
            // We grab the last 10 succeeded jobs and see if any of them contain our batchId in their arguments
            var succeededJobs = api.SucceededJobs(0, 10);

            bool isDone = succeededJobs.Any(j =>
                j.Value.Job?.Args.Any(arg => arg.ToString().Contains(batchId)) == true
            );

            if (isDone) return;

            // Fail fast if the job crashed so you don't wait the full 120 seconds
            var failedJobs = api.FailedJobs(0, 10);
            bool isFailed = failedJobs.Any(j =>
                j.Value.Job?.Args.Any(arg => arg.ToString().Contains(batchId)) == true
            );

            if (isFailed) throw new Exception("Hangfire job failed during execution!");

            // 🚀 Poll every 100ms for highly accurate graph data
            await Task.Delay(100);
        }

        throw new TimeoutException("Hangfire job took too long!");
    }

    [Fact]
    public async Task BulkInsertEmployees_WithValidData_ShouldReturnAcceptedAndProcessAll()
    {
        const int employeesCount = 2000;
        // Arrange
        var employees = TestDataGenerator.Employee
            .ForProperty(e => e.CreatedBy, _userClient.User.Id)
            .Generate(employeesCount);

        // Act
        var batchId = await PostBulkInsertAsync(employees);

        // Assert
        await WaitForEmployeesAndTokensAsync(employeesCount);

        var addedCount = await _dbUtility.Query<Employee>()
            .Where(e => e.CreatedBy == _userClient.User.Id)
            .CountAsync();

        addedCount.Should().Be(employeesCount);
    }

    /*[Fact]
    public async Task SendRequest()
    {
        const int employeesCount = 2000;
        // Arrange
        var employees = TestDataGenerator.Employee
            .ForProperty(e => e.CreatedBy, _userClient.User.Id)
            .Generate(employeesCount);

        // Act
        var batchId = await PostBulkInsertAsync(employees);
    }*/

    [Fact]
    public async Task BulkInsertEmployees_WithDuplicateNationalNumberInRequest_ShouldHandleGracefully()
    {
        // Arrange
        var baseEmployee = TestDataGenerator.Employee
            .ForProperty(e => e.CreatedBy, _userClient.User.Id)
            .WithValue(EmployeeFields.NationalNumber)
            .Generate();

        var duplicateEmployee = TestDataGenerator.Employee
            .ForProperty(e => e.CreatedBy, _userClient.User.Id)
            .ForProperty(e => e.NationalNumber, baseEmployee.NationalNumber)
            .Generate();

        var employees = new List<Employee> { baseEmployee, duplicateEmployee };

        // Act
        var batchId = await PostBulkInsertAsync(employees);

        // Assert
        // Since one is a duplicate in the request, the background job should only insert 1
        await WaitForEmployeesAndTokensAsync(1);

        var addedCount = await _dbUtility.Query<Employee>()
            .Where(e => e.CreatedBy == _userClient.User.Id)
            .CountAsync();

        addedCount.Should().Be(1);
    }

    [Fact]
    public async Task BulkInsertEmployees_WithDuplicateNationalNumberInDatabase_ShouldHandleGracefully()
    {
        // Arrange
        var existingEmployee = TestDataGenerator.Employee
            .ForProperty(e => e.CreatedBy, _userClient.User.Id)
            .WithValue(EmployeeFields.NationalNumber)
            .Generate();

        var createdEmployee = await ApiManager.CreateEmployee(existingEmployee, _userClient.Client);
        createdEmployee.Should().NotBeNull();

        var duplicateEmployee = TestDataGenerator.Employee
            .ForProperty(e => e.CreatedBy, _userClient.User.Id)
            .ForProperty(e => e.NationalNumber, existingEmployee.NationalNumber)
            .Generate();

        // Act
        var batchId = await PostBulkInsertAsync(new[] { duplicateEmployee });

        // Assert
        // Since the employee already exists in the DB, the background job should insert 0 new employees.
        // We wait a fixed time to ensure the background job has fully processed the batch.
        await Task.Delay(10000); // Wait 10 seconds for background job to finish

        var allEmployees = await _dbUtility.Query<Employee>()
            .Where(e => e.CreatedBy == _userClient.User.Id)
            .ToListAsync();

        allEmployees.Should().HaveCount(1); // Only the existing employee should remain
    }

    [Fact]
    public async Task BulkInsertEmployees_WithInvalidData_ShouldHandleGracefully()
    {
        // Arrange
        var validEmployees = TestDataGenerator.Employee
            .ForProperty(e => e.CreatedBy, _userClient.User.Id)
            .Generate(2);

        var invalidEmployee = TestDataGenerator.Employee
            .ForProperty(e => e.CreatedBy, _userClient.User.Id)
            .ForProperty(e => e.Name, "") // Invalid: empty name
            .Generate();

        var employees = validEmployees.Append(invalidEmployee).ToList();

        // Act
        var batchId = await PostBulkInsertAsync(employees);

        // Assert
        // Only the 2 valid employees should be inserted
        await WaitForEmployeesAndTokensAsync(2);

        var addedCount = await _dbUtility.Query<Employee>()
            .Where(e => e.CreatedBy == _userClient.User.Id)
            .CountAsync();

        addedCount.Should().Be(2);
    }

    #region Helper Methods

    private BatchEmployee BulkInsertEmployeeObject(Employee employee)
    {
        return new BatchEmployee
        (
            Tracker: Guid.NewGuid(),
            Name: employee.Name,
            NationalNumber: employee.NationalNumber,
            AccountNumber: employee.AccountNumber,
            Salary: employee.Salary
        );
    }

    private BulkInsert.Request BulkInsertEmployeeRequest(IEnumerable<Employee> employees)
    {
        return new BulkInsert.Request(
            employees
                .Select(e => BulkInsertEmployeeObject(e))
                .ToList());
    }

    #endregion
}