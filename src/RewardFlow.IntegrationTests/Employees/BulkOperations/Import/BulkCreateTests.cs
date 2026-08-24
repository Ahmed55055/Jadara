using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Reward_Flow_v2.Employees.BulkInsertEmployees.CkeckBatchResult;
using Reward_Flow_v2.Employees.Data;
using RewardFlow.IntegrationTests.Employees.Common;
using RewardFlow.IntegrationTests.Infrastructure;
using RewardFlow.IntegrationTests.Infrastructure.Requesters;
using RewardFlow.TestUtilities.DataGenerators;
using RewardFlow.TestUtilities.DataGenerators.Fakers.Employees;
using RewardFlow.TestUtilities.UtilityClasses;
using System.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace RewardFlow.IntegrationTests.Employees.BulkOperations.Import;

public class BulkCreateTests(TestWebApplicationFactory factory, ITestOutputHelper _output)
    : BaseEmployeeTestFixture(factory,_output), IAsyncLifetime
{
    #region Helpers
    
    private static void AssertBatchResult(BatchResult actual, BatchResult expected)
    {
        actual.TotalRecords.Should().Be(expected.TotalRecords);
        actual.TotalSucceeded.Should().Be(expected.TotalSucceeded);
        actual.Failed.Should().HaveCount(expected.Failed.Length);

        for (var i = 0; i < expected.Failed.Length; i++)
        {
            var actualFailure = actual.Failed[i];
            var expectedFailure = expected.Failed[i];

            actualFailure.Reason.Should().Contain(expectedFailure.Reason);
            actualFailure.Message.ToLower().Should().Contain(expectedFailure.Message.ToLower());
        }
    }

    private BatchResult CreateBatchResult(int totalRecords, int totalSucceeded, params FailedRecord[] failedRecord)
    {
        return new BatchResult(
            TotalRecords: totalRecords,
            TotalSucceeded: totalSucceeded,
            Failed: failedRecord);
    }

    private FailedRecord CreateFailedRecord(string reason, string message)
    {
        return new FailedRecord(TrackerId: Guid.Empty, Reason: reason, Message: message);
    }
    #endregion

    [Fact]
    public async Task BulkInsertEmployees_SpeedTest_MeasureExecutionTime()
    {
        const int employeesCount = 200;

        // Arrange
        var employees = TestDataGenerator.Employee
            .ForProperty(e => e.CreatedBy, _userClient.User.Id)
            .Generate(employeesCount);

        // Act
        var stopwatch = Stopwatch.StartNew();

        var batchId = await _employeeApi.ImportAsync(employees);

        await WaitForHangfireJobFromFactoryAsync(batchId.ToString());

        stopwatch.Stop();

        var batchResult = await _employeeApi.GetBatchResult(batchId);
        
        // Assert

        AssertBatchResult(
            batchResult!,
            CreateBatchResult(employeesCount, employeesCount));

        _output.WriteLine(
            $"✅ SPEED TEST RESULT: Processed {employeesCount} employees in {stopwatch.Elapsed.TotalSeconds:F2} seconds.");
        
        
        // This method used only for speed measurements so we don't need it to take space of the actual test class
        // fun part should not interfere with the actual work
        async Task WaitForHangfireJobFromFactoryAsync(string batchId)
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
    }


    [Fact]
    public async Task BulkInsertEmployees_WithValidData_ShouldReturnAcceptedAndProcessAll()
    {
        const int employeesCount = 200;

        // Arrange
        var employees = TestDataGenerator.Employee
            .ForProperty(e => e.CreatedBy, _userClient.User.Id)
            .Generate(employeesCount);

        // Act
        var batchId = await _employeeApi.ImportAsync(employees);
        await _importWaiter.WaitForImportProccessing(employeesCount);
        var batchResult = await _employeeApi.GetBatchResult(batchId);

        // Assert

        var addedCount = await _dbUtility.Query<Employee>()
            .Where(e => e.CreatedBy == _userClient.User.Id)
            .CountAsync();

        addedCount.Should().Be(employeesCount);

        AssertBatchResult(
            batchResult!,
            CreateBatchResult(employeesCount, employeesCount));
    }


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
        var batchId = await _employeeApi.ImportAsync(employees);
        await _importWaiter.WaitForImportProccessing(1);
        var batchResult = await _employeeApi.GetBatchResult(batchId);

        // Assert

        var addedCount = await _dbUtility.Query<Employee>()
            .Where(e => e.CreatedBy == _userClient.User.Id)
            .CountAsync();

        addedCount.Should().Be(1);


        AssertBatchResult(
            batchResult!,
            CreateBatchResult(2, 1, CreateFailedRecord("Duplicate", "National")));
    }


    [Fact]
    public async Task BulkInsertEmployees_WithDuplicateNationalNumberInDatabase_ShouldHandleGracefully()
    {
        // Arrange
        var existingEmployee = TestDataGenerator.Employee
            .ForProperty(e => e.CreatedBy, _userClient.User.Id)
            .WithValue(EmployeeFields.NationalNumber)
            .Generate();

        var createdEmployee = await _employeeApi.CreateEmployee(existingEmployee);

        createdEmployee.Should().NotBeNull();

        var duplicateEmployee = TestDataGenerator.Employee
            .ForProperty(e => e.CreatedBy, _userClient.User.Id)
            .ForProperty(e => e.NationalNumber, existingEmployee.NationalNumber)
            .Generate();

        // Act
        var batchId = await _employeeApi.ImportAsync(new[] { duplicateEmployee });
        await Task.Delay(5000);
        var batchResult = await _employeeApi.GetBatchResult(batchId);

        // Assert

        var allEmployees = await _dbUtility.Query<Employee>()
            .Where(e => e.CreatedBy == _userClient.User.Id)
            .ToListAsync();

        allEmployees.Should().HaveCount(1);
        
        AssertBatchResult(
            batchResult!,
            CreateBatchResult(1, 0, CreateFailedRecord("Conflict", "National")));
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
            .ForProperty(e => e.Name, "")
            .Generate();

        var employees = validEmployees
            .Append(invalidEmployee)
            .ToList();

        // Act
        var batchId = await _employeeApi.ImportAsync(employees);
        await _importWaiter.WaitForImportProccessing(2);
        var batchResult = await _employeeApi.GetBatchResult(batchId);

        // Assert

        var addedCount = await _dbUtility.Query<Employee>()
            .Where(e => e.CreatedBy == _userClient.User.Id)
            .CountAsync();

        addedCount.Should().Be(2);
        
        AssertBatchResult(
            batchResult!,
            CreateBatchResult(3, 2, CreateFailedRecord("Name", "Name")));
    }
}