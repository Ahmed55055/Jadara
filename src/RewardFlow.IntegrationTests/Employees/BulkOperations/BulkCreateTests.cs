using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RewardFlow.IntegrationTests.Infrastructure;
using Reward_Flow_v2.Employees.Data;
using RewardFlow.IntegrationTests.Infrastructure.DataGenerators;
using Reward_Flow_v2.Employees.CreateEmployee;
using Reward_Flow_v2.Employees.BulkInsertEmployees;
using RewardFlow.IntegrationTests.Employees.Common;
using RewardFlow.IntegrationTests.Infrastructure.DataGenerators.Fakers.Employees;
using Xunit;

namespace RewardFlow.IntegrationTests.Employees.BulkOperations;

[Collection("EmployeeTests")]
public class BulkCreateTests : IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;
    private readonly DbUtility _dbUtility;
    private UserClient _userClient;

    public BulkCreateTests(EmployeeTestFixture factory)
    {
        _factory = factory;
        _dbUtility = new DbUtility(_factory);
    }

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
        var employees = TestDataGenerator.Employee
            .ForProperty(e => e.CreatedBy, _userClient.User.Id)
            .Generate(2);

        BulkInsert.Request bulkRequest = BulkInsertEmployeeRequest(employees);

        // Act
        var response = await _userClient.Client.PostAsJsonAsync("/api/Employees/BulkInsert", bulkRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var result = await response.Content.ReadFromJsonAsync<BulkInsert.Response>();
        result.Should().NotBeNull();
        result!.Success.Should().Be(2);
        result.FailsIndexes.Should().BeEmpty();

        // Verify employees were created
        var allEmployees = await _dbUtility.Set<Employee>().ToListAsync();
        allEmployees!.Should().Contain(e => e.Name == employees[0].Name && e.CreatedBy == _userClient.User.Id);
        allEmployees.Should().Contain(e => e.Name == employees[1].Name && e.CreatedBy == _userClient.User.Id);
    }

    [Fact]
    public async Task BulkInsertEmployees_WithDuplicateNationalNumbers_ShouldReturnBadRequest()
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
            .Generate(3);

        var bulkRequest = BulkInsertEmployeeRequest(duplicateEmployee);

        // Act
        var response = await _userClient.Client.PostAsJsonAsync("/api/Employees/BulkInsert", bulkRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var result = await response.Content.ReadFromJsonAsync<BulkInsert.Response>();
        result.Should().NotBeNull();
        result!.Success.Should().Be(0);
        result.FailsIndexes.Should().Contain(0);
        
        // Verify that no employees were inserted due to duplicate national numbers
        var allEmployees = await _dbUtility.Set<Employee>().ToListAsync();
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
        result!.Success.Should().Be(2); // Only 2 valid employees should be inserted
        result.FailsIndexes.Should().Contain(2); // The invalid one at index 2
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

    private BulkInsert.emp BulkInsertEmployeeObject(Employee employee)
    {
        return new BulkInsert.emp
        (
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