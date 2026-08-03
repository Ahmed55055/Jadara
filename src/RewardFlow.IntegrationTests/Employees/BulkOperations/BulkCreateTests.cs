using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
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

namespace RewardFlow.IntegrationTests.Employees.BulkOperations;

public class BulkCreateTests(TestWebApplicationFactory factory, ITestOutputHelper output) : BaseEmployeeTestFixture(factory), IAsyncLifetime
{
    private UserClient _userClient;

    public async Task InitializeAsync()
    {
        var user = TestDataGenerator.User.Generate();
        await _dbUtility.InsertAsync(user);
        _userClient = new UserClient(_factory, user);
        _userClient.Authanticate();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task BulkInsertEmployees_WithValidData_ShouldReturnOk()
    {
        // Arrange
        const int employeesCount = 2000;
        
        var employees = TestDataGenerator.Employee
            .ForProperty(e => e.CreatedBy, _userClient.User.Id)
            .Generate(employeesCount);

        BulkInsert.Request bulkRequest = BulkInsertEmployeeRequest(employees);


        // Act
        var stopwatch = Stopwatch.StartNew();
        
        var response = await _userClient.Client.PostAsJsonAsync("/api/Employees/BulkInsert", bulkRequest);
        
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var result = await response.Content.ReadFromJsonAsync<BulkInsert.Response>();
        result.Should().NotBeNull();
        result!.Summary.SuccessfulRecords.Should().Be(employeesCount);
        result.Errors.Should().BeEmpty();

        // Verify employees were created
        var addedCount = await _dbUtility.Query<Employee>().Where(e=>e.CreatedBy == _userClient.User.Id).CountAsync();
        addedCount.Should().Be(employees.Count);
        
        output.WriteLine(
            $"✅ SPEED TEST RESULT: Processed {employeesCount} employees in {stopwatch.Elapsed.TotalSeconds:F2} seconds.");
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
        var bulkRequest = BulkInsertEmployeeRequest(employees);

        // Act
        var response = await _userClient.Client.PostAsJsonAsync("/api/Employees/BulkInsert", bulkRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var result = await response.Content.ReadFromJsonAsync<BulkInsert.Response>();
        result.Should().NotBeNull();
        result!.Summary.SuccessfulRecords.Should().Be(1);
        result.Summary.FailedRecords.Should().Be(1);
        result.Errors.Should().ContainSingle(e => e.ErrorStatusCode == BulkInsert.ErrorTypes.DuplicateNationalNumber);
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

        var bulkRequest = BulkInsertEmployeeRequest(new[] { duplicateEmployee });

        // Act
        var response = await _userClient.Client.PostAsJsonAsync("/api/Employees/BulkInsert", bulkRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var result = await response.Content.ReadFromJsonAsync<BulkInsert.Response>();
        result.Should().NotBeNull();
        result!.Summary.SuccessfulRecords.Should().Be(0);
        result.Summary.FailedRecords.Should().Be(1);
        result.Errors.Should().ContainSingle(e => e.ErrorStatusCode == BulkInsert.ErrorTypes.DatabaseConflict);
        
        // Verify that no new employees were inserted due to duplicate national numbers
        var allEmployees = await _dbUtility.Query<Employee>().Where(e=>e.CreatedBy == _userClient.User.Id).ToListAsync();
        allEmployees.Should().HaveCount(1); // Only the existing employee
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

        var bulkRequest = BulkInsertEmployeeRequest(validEmployees.Append(invalidEmployee));

        // Act
        var response = await _userClient.Client.PostAsJsonAsync("/api/Employees/BulkInsert", bulkRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var result = await response.Content.ReadFromJsonAsync<BulkInsert.Response>();
        result.Should().NotBeNull();
        result!.Summary.SuccessfulRecords.Should().Be(2); // Only 2 valid employees should be inserted
        result.Errors.Should().ContainSingle(e => e.ErrorStatusCode == BulkInsert.ErrorTypes.InvalidName);
    }

    private CreateEmployee.Request CreateEmployeeRequest(Employee employee)
    {
        return new CreateEmployee.Request
        (
            Name: employee.Name,
            NationalNumber: employee.NationalNumber,
            AccountNumber: employee.AccountNumber,
            Salary: employee.Salary,
            FacultyId: employee.FacultyId,
            DepartmentId: employee.DepartmentId,
            JobTitle: employee.JobTitle,
            Status: employee.Status
        );
    }

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
}