using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Reward_Flow_v2.Common;
using Reward_Flow_v2.Employees.Common;
using RewardFlow.IntegrationTests.Infrastructure;
using Reward_Flow_v2.Employees.Data;
using Reward_Flow_v2.Employees.UpdateEmployee;
using RewardFlow.IntegrationTests.Employees.Common;
using RewardFlow.TestUtilities.DataGenerators;
using System.Reflection;
using Xunit;

namespace RewardFlow.IntegrationTests.Employees;

[Collection("EmployeeTests")]
public class UpdateEmployeeTests : IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;
    private readonly DbUtility _dbUtility;
    private UserClient _userClient;

    public UpdateEmployeeTests(EmployeeTestFixture factory)
    {
        _factory = factory;
        _dbUtility = new DbUtility(_factory);
    }

    public async Task InitializeAsync()
    {
        var user = TestDataGenerator.User.Generate();
        await _dbUtility.InsertAsync(user);
        _userClient = new UserClient(_factory, user);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task UpdateEmployee_WithValidData_ShouldReturnNoContent()
    {
        // Arrange
        var employeeData = TestDataGenerator.Employee
            .ForProperty(e => e.CreatedBy, _userClient.User.Id)
            .Generate();
        var createdEmployee = await ApiManager.CreateEmployee(employeeData, _userClient.Client);

        // Generate another employee to get updated values from
        var updatedEmployeeData = TestDataGenerator.Employee.Generate();

        var updateRequest = new UpdateEmployee.Request
        {
            Name = updatedEmployeeData.Name,
            NationalNumber = updatedEmployeeData.NationalNumber,
            AccountNumber = updatedEmployeeData.AccountNumber,
            Salary = updatedEmployeeData.Salary,
            FacultyId = updatedEmployeeData.FacultyId,
            DepartmentId = updatedEmployeeData.DepartmentId,
            JobTitle = updatedEmployeeData.JobTitle,
            Status = updatedEmployeeData.Status
        };

        // Act
        var response =
            await _userClient.Client.PatchAsJsonAsync($"/api/Employees/{createdEmployee.Id}", updateRequest);

        // Assert
        await AssertEmployeeUpdateSuccess(response, createdEmployee.Id, updateRequest, employeeData);
    }

    [Fact]
    public async Task UpdateEmployee_WithPartialData_ShouldUpdateOnlySpecifiedFields()
    {
        // Arrange
        var employeeData = TestDataGenerator.Employee
            .ForProperty(e => e.CreatedBy, _userClient.User.Id)
            .Generate();
        var createdEmployee = await ApiManager.CreateEmployee(employeeData, _userClient.Client);

        var updateRequest = new UpdateEmployee.Request
        {
            Name = "Updated Name"
            // Other fields are not set (default Optional with no value)
        };

        // Act
        var response =
            await _userClient.Client.PatchAsJsonAsync($"/api/Employees/{createdEmployee.Id}", updateRequest);

        // Assert
        await AssertEmployeeUpdateSuccess(response, createdEmployee.Id, updateRequest, employeeData);
    }

    [Fact]
    public async Task UpdateEmployee_WithDuplicateNationalNumber_ShouldReturnConflict()
    {
        // Arrange
        var employeeData = TestDataGenerator.Employee
            .ForProperty(e => e.CreatedBy, _userClient.User.Id)
            .Generate();
        var createdEmployee = await ApiManager.CreateEmployee(employeeData, _userClient.Client);

        var otherEmployee = TestDataGenerator.Employee
            .ForProperty(e => e.CreatedBy, _userClient.User.Id)
            .Generate();
        var otherCreatedEmployee = await ApiManager.CreateEmployee(otherEmployee, _userClient.Client);

        var updateRequest = new UpdateEmployee.Request
        {
            NationalNumber = otherEmployee.NationalNumber // Duplicate national number
        };

        // Act
        var response =
            await _userClient.Client.PatchAsJsonAsync($"/api/Employees/{createdEmployee.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task UpdateEmployee_WithDuplicateAccountNumber_ShouldReturnConflict()
    {
        // Arrange
        var employeeData = TestDataGenerator.Employee
            .ForProperty(e => e.CreatedBy, _userClient.User.Id)
            .Generate();

        var otherEmployee = TestDataGenerator.Employee
            .ForProperty(e => e.CreatedBy, _userClient.User.Id)
            .Generate();
        
        var createdEmployee = ApiManager.CreateEmployee(employeeData,_userClient.Client);
        var otherCreatedEmployee = ApiManager.CreateEmployee(otherEmployee,_userClient.Client);

        var updateRequest = new UpdateEmployee.Request
        {
            AccountNumber = otherEmployee.AccountNumber // Duplicate account number
        };

        // Act
        var response =
            await _userClient.Client.PatchAsJsonAsync($"/api/Employees/{createdEmployee.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task UpdateEmployee_WithNonExistentDepartmentId_ShouldReturnBadRequest()
    {
        // Arrange
        var employeeData = TestDataGenerator.Employee
            .ForProperty(e => e.CreatedBy, _userClient.User.Id)
            .Generate();
        var createdEmployee = await ApiManager.CreateEmployee(employeeData, _userClient.Client);

        var updateRequest = new UpdateEmployee.Request
        {
            DepartmentId = 999999 // Non-existent department ID
        };

        // Act
        var response =
            await _userClient.Client.PatchAsJsonAsync($"/api/Employees/{createdEmployee.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain("Department");
    }

    [Fact]
    public async Task UpdateEmployee_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var updateRequest = new UpdateEmployee.Request { Name = "Updated Name" };

        // Act
        var response = await _userClient.Client.PatchAsJsonAsync("/api/Employees/99999", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateEmployee_CreatedByDifferentUser_ShouldReturnNotFound()
    {
        // Arrange
        var otherUser = TestDataGenerator.User.Generate();
        await _dbUtility.InsertAsync(otherUser);

        var employeeData = TestDataGenerator.Employee
            .ForProperty(e => e.CreatedBy, otherUser.Id)
            .Generate();
        await _dbUtility.InsertAsync(employeeData);

        var updateRequest = new UpdateEmployee.Request { Name = "Updated Name" };

        // Act
        var response =
            await _userClient.Client.PatchAsJsonAsync($"/api/Employees/{employeeData.EmployeeId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateEmployee_CreatedByTwoUsers_ShouldUpdateOnlyOwnedUser()
    {
        // Arrange
        var otherUser = TestDataGenerator.User.Generate();
        await _dbUtility.InsertAsync(otherUser);
        var otherUserClient = new UserClient(_factory, otherUser);
        otherUserClient.Authanticate();

        var employeeData1 = TestDataGenerator.Employee
            .ForProperty(e => e.CreatedBy, _userClient.User.Id)
            .Generate();
        var createdEmployee1 = await ApiManager.CreateEmployee(employeeData1, _userClient.Client);

        var employeeData2 = TestDataGenerator.Employee
            .ForProperty(e => e.CreatedBy, otherUser.Id)
            .Generate();
        var createdEmployee2 = await ApiManager.CreateEmployee(employeeData2, otherUserClient.Client);

        var updateRequest = new UpdateEmployee.Request { Name = "Updated Name" };

        // Act
        var response1 =
            await _userClient.Client.PatchAsJsonAsync($"/api/Employees/{createdEmployee1.Id}", updateRequest);
        var response2 =
            await otherUserClient.Client.PatchAsJsonAsync($"/api/Employees/{createdEmployee2.Id}", updateRequest);

        // Assert
        await AssertEmployeeUpdateSuccess(response1, createdEmployee1.Id, updateRequest, employeeData1);
        await AssertEmployeeUpdateSuccess(response2, createdEmployee2.Id, updateRequest, employeeData2);
    }

    [Fact]
    public async Task UpdateEmployee_WithNullOptionalValues_ShouldUpdateToNull()
    {
        // Arrange
        var employeeData = TestDataGenerator.Employee
            .ForProperty(e => e.CreatedBy, _userClient.User.Id)
            .Generate();
        var createdEmployee = await ApiManager.CreateEmployee(employeeData, _userClient.Client);

        var updateRequest = new UpdateEmployee.Request
        {
            NationalNumber = null, // Explicitly set to null
            AccountNumber = null // Explicitly set to null
        };

        // Act
        var response =
            await _userClient.Client.PatchAsJsonAsync($"/api/Employees/{createdEmployee.Id}", updateRequest);

        // Assert
        await AssertEmployeeUpdateSuccess(response, createdEmployee.Id, updateRequest, employeeData);
    }

    /// <summary>
    /// Asserts that an employee update was successful by validating the response and the updated employee data.
    /// </summary>
    /// <param name="response">The HTTP response from the employee update request</param>
    /// <param name="employeeId">The ID of the employee being updated</param>
    /// <param name="request">The request data used to update the employee</param>
    /// <param name="originalEmployee">The original employee data before the update</param>
    private async Task AssertEmployeeUpdateSuccess(HttpResponseMessage response, int employeeId,
        UpdateEmployee.Request request, Employee originalEmployee)
    {
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var updatedEmployee = await _dbUtility.Set<Employee>().FindAsync(employeeId);
        updatedEmployee.Should().NotBeNull();
        
        List<string> fieldsNames = new();

        foreach (var property in typeof(Employee).Properties())
            fieldsNames.Add(property.Name);

        AssertAllUpdates(request, updatedEmployee, originalEmployee, fieldsNames.ToArray());
    }

    private void AssertAllUpdates(UpdateEmployee.Request request, Employee updated, Employee original,
        params string[] properties)
    {
        var defaultOptional = new Optional<object?>();
        defaultOptional.HasValue = false;

        foreach (var propName in properties)
        {
            var requestProp = request.GetType().GetProperty(propName);
            var EmpProp = typeof(Employee).GetProperty(propName);

            if (EmpProp is null)
                continue;

            var requestValue = requestProp is null ? (object)defaultOptional : requestProp.GetValue(request);
            var updatedValue = EmpProp.GetValue(updated);
            var originalValue = EmpProp.GetValue(original);

            AssertUpdateValue(requestValue, updatedValue, originalValue);
        }
    }

    private void AssertUpdateValue<T>(Optional<T> requestValue, T updatedValue, T originalValue)
    {
        if (requestValue.HasValue)
            updatedValue.Should().Be(requestValue.Value);
        else
            updatedValue.Should().Be(originalValue);
    }
}