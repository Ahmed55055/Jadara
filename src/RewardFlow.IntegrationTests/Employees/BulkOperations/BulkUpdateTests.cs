using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RewardFlow.IntegrationTests.Infrastructure;
using Reward_Flow_v2.Employees.Data;
using Reward_Flow_v2.Employees.CreateEmployee;
using Reward_Flow_v2.Employees.BulkInsertEmployees;
using Reward_Flow_v2.Employees.UpdateEmployee;
using RewardFlow.IntegrationTests.Employees.Common;
using RewardFlow.TestUtilities.DataGenerators;
using Xunit;

namespace RewardFlow.IntegrationTests.Employees.BulkOperations;

public class BulkUpdateTests(TestWebApplicationFactory factory) : BaseEmployeeTestFixture(factory), IAsyncLifetime
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
    public async Task BulkUpdateEmployees_WithValidData_ShouldReturnAccepted()
    {
        // Arrange
        var employees = TestDataGenerator.Employee
            .ForProperty(e => e.CreatedBy, _userClient.User.Id)
            .Generate(3);

        foreach (var employee in employees)
        {
            await ApiManager.CreateEmployee(employee, _userClient.Client);
        }

        var updateRequests = employees.Select(e => new UpdateEmployee.Request
        {
            Name = "Updated Name"
        }).ToList();

        // Act
        var response = await _userClient.Client.PutAsJsonAsync("/api/Employees/BulkUpdate", updateRequests);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var result = await response.Content.ReadFromJsonAsync<BulkInsert.Response>();
        result.Should().NotBeNull();
        result!.Summary.SuccessfulRecords.Should().Be(3);
        result.Errors.Should().BeEmpty();

        // Verify employees were updated
        var allEmployees = await _dbUtility.Query<Employee>().ToListAsync();
        allEmployees.Should().Contain(e => e.Name == "Updated Name" && e.CreatedBy == _userClient.User.Id);
        
        // Verify tokens were updated in the Employee Name Tokens table
        var tokensExist = await _dbUtility.Query<EmployeeNameToken>().AnyAsync(
            t => t.EmployeeId == allEmployees[0].EmployeeId && t.UserId == _userClient.User.Id);
        tokensExist.Should().BeTrue();
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
}