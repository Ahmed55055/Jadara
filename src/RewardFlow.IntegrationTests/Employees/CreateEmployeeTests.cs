using Bogus;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Reward_Flow_v2.Employees.Common;
using Reward_Flow_v2.Employees.CreateEmployee;
using RewardFlow.IntegrationTests.Infrastructure;
using Reward_Flow_v2.Employees.Data;
using RewardFlow.IntegrationTests.Employees.Common;
using RewardFlow.IntegrationTests.Infrastructure.DataGenerators;
using Xunit;

namespace RewardFlow.IntegrationTests.Employees;

/// <summary>
/// Integration tests for the Create Employee endpoint.
/// Tests various scenarios for creating employees including validation, duplicate detection, and concurrent operations.
/// </summary>
[Collection("EmployeeTests")]
public class CreateEmployeeTests : IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;
    private readonly DbUtility _dbUtility;
    private UserClient _userClient;
    public CreateEmployeeTests(EmployeeTestFixture factory)
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
    public async Task CreateEmployee_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        await _dbUtility.InsertRangeAsync(TestDataGenerator.Employee.Generate(20));

        var employeeData = TestDataGenerator.Employee.Generate();
        var request = RequestCreator.CreateEmployeeRequest(employeeData);

        // Act
        var response = await _userClient.Client.PostAsJsonAsync("/api/Employees", request);

        // Assert
        await AssertEmployeeCreationSuccess(response, request);
    }

    /// <summary>
    /// Tests that creating an employee with a non-existent department ID returns HTTP 400 Bad Request
    /// and includes appropriate error messages.
    /// </summary>
    [Fact]
    public async Task CreateEmployee_WithNonExistentDepartmentId_ShouldReturnBadRequest()
    {
        // Arrange
        var employeeData = TestDataGenerator.Employee
            .ForProperty(e => e.DepartmentId, 999999) // Non-existent department ID
            .Generate();
        var request = RequestCreator.CreateEmployeeRequest(employeeData);

        // Act
        var response = await _userClient.Client.PostAsJsonAsync("/api/Employees", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain("Department");
    }


    /// <summary>
    /// Tests that creating an employee with a duplicate national number returns HTTP 409 Conflict.
    /// This ensures data integrity by preventing duplicate national numbers for the same user.
    /// </summary>
    [Fact]
    public async Task CreateEmployee_WithDuplicateNationalNumber_ShouldReturnConflict()
    {
        // Arrange
        var existingEmployee = TestDataGenerator.Employee
            .WithValue(EmployeeFields.NationalNumber)
            .ForProperty(e=>e.CreatedBy,_userClient.User.Id)
            .Generate();
        
        await _dbUtility.InsertAsync(existingEmployee);

        var requestEmployee = TestDataGenerator.Employee
            .ForProperty(e=>e.NationalNumber,existingEmployee.NationalNumber)
            .Generate();
        
        var request = RequestCreator.CreateEmployeeRequest(requestEmployee);

        // Act
        var response = await _userClient.Client.PostAsJsonAsync("/api/Employees", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    /// <summary>
    /// Tests that different users can create employees with the same data without conflicts.
    /// This verifies that employee data is scoped to individual users.
    /// </summary>
    [Fact]
    public async Task CreateEmployee_WithSameData_DifferentUsers_ShouldReturnCreated()
    {
        // Arrange
        var user1 = TestDataGenerator.User.Generate();
        var user2 = TestDataGenerator.User.Generate();
        await _dbUtility.InsertAsync(user1);
        await _dbUtility.InsertAsync(user2);
        var UserClient1 = new UserClient(_factory, user1);
        UserClient1.Authanticate();
        var UserClient2 = new UserClient(_factory, user2);
        UserClient2.Authanticate();

        var baseData = TestDataGenerator.Employee.Generate();
        var request1 = RequestCreator.CreateEmployeeRequest(baseData);

        var uniqueData2 = TestDataGenerator.Employee.Generate();
        var request2 = RequestCreator.CreateEmployeeRequest(uniqueData2);

        // Act
        var response1 = await UserClient1.Client.PostAsJsonAsync("/api/Employees", request1);
        var response2 = await UserClient2.Client.PostAsJsonAsync("/api/Employees", request2);

        // Assert
        await AssertEmployeeCreationSuccess(response1, request1);
        await AssertEmployeeCreationSuccess(response2, request2);
    }
    /// <summary>
    /// Tests that creating an employee with a duplicate account number returns HTTP 409 Conflict.
    /// This ensures data integrity by preventing duplicate account numbers for the same user.
    /// </summary>
    [Fact]
    public async Task CreateEmployee_WithDuplicateAccountNumber_ShouldReturnConflict()
    {
        // Arrange
        var existingEmployee = TestDataGenerator.Employee            
            .ForProperty(e=>e.CreatedBy,_userClient.User.Id)
            .Generate();
        
        await _dbUtility.InsertAsync(existingEmployee);

        var requestEmployee = TestDataGenerator.Employee
            .ForProperty(e => e.AccountNumber, existingEmployee.AccountNumber)
            .Generate();

        var request = RequestCreator.CreateEmployeeRequest(requestEmployee);

        // Act
        var response = await _userClient.Client.PostAsJsonAsync("/api/Employees", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
    /// <summary>
    /// Tests concurrent creation attempts with the same employee data.
    /// Verifies that only one creation succeeds and subsequent attempts return HTTP 409 Conflict,
    /// ensuring thread safety and data integrity.
    /// </summary>
    [Fact]
    public async Task CreateEmployee_ConcurrentCalls_WithSameData_ShouldAllowOnlyOne()
    {
        // Arrange
        var employeeData = TestDataGenerator.Employee
            .ForProperty(e=>e.CreatedBy, _userClient.User.Id)
            .WithValue(EmployeeFields.NationalNumber)
            .Generate();
        var request = RequestCreator.CreateEmployeeRequest(employeeData);

        // Act
        var tasks = new List<Task<HttpResponseMessage>>();
        for (int i = 0; i < 20; i++)
        {
            tasks.Add(_userClient.Client.PostAsJsonAsync("/api/Employees", request));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert
        var createdCount = responses.Count(r => r.StatusCode == HttpStatusCode.Created);
        var conflictCount = responses.Count(r => r.StatusCode == HttpStatusCode.Conflict);
        createdCount.Should().Be(1);
        conflictCount.Should().Be(19);
    }

    [Fact]
    public async Task CreateEmployee_WithArabicName_ShouldReturnCreated()
    {
        // Arrange
        var employeeData = TestDataGenerator.Employee.Generate();
        employeeData.Name = "محمد أحمد"; // Arabic name
        var request = RequestCreator.CreateEmployeeRequest(employeeData);

        // Act
        var response = await _userClient.Client.PostAsJsonAsync("/api/Employees", request);

        // Assert
        await AssertEmployeeCreationSuccess(response, request);
    }

    /// <summary>
    /// Asserts that an employee creation was successful by validating the response and optionally the employee data.
    /// </summary>
    /// <param name="response">The HTTP response from the employee creation request</param>
    /// <param name="request">The request data used to create the employee</param>
    private async Task AssertEmployeeCreationSuccess(HttpResponseMessage response, CreateEmployee.Request request)
    {
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var employee = await response.Content.ReadFromJsonAsync<EmployeeDto>();
        employee.Should().NotBeNull();
        employee.Name.Should().Be(request.Name);
        employee.NationalNumber.Should().Be(request.NationalNumber);

        employee.CreatedAt.Should().BeOnOrBefore(DateTime.UtcNow);
        employee.CreatedAt.Should().BeOnOrAfter(DateTime.UtcNow.AddSeconds(-5));

        // Verify that name tokens were created for fuzzy search functionality
        var tokensExist = await _dbUtility.Set<EmployeeNameToken>().AnyAsync(
            t => t.EmployeeId == employee.Id && t.UserId == _userClient.User.Id);
        tokensExist.Should().BeTrue();
    }
}