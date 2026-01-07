using Bogus;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Reward_Flow_v2.Employees.CreateEmployee;
using RewardFlow.IntegrationTests.Infrastructure;
using Reward_Flow_v2.Employees.Data;
using RewardFlow.IntegrationTests.Infrastructure.DataGenerators;
using Xunit;

namespace RewardFlow.IntegrationTests.Employees;

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
        _userClient.Authanticate();
    }
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CreateEmployee_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        await _dbUtility.InsertRangeAsync(TestDataGenerator.Employee.Generate(20));

        var employeeData = TestDataGenerator.Employee.Generate();
        var request = CreateEmployeeRequest(employeeData);

        // Act
        var response = await _userClient.Client.PostAsJsonAsync("/api/Employees", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var employee = await response.Content.ReadFromJsonAsync<Employee>();
        employee.Should().NotBeNull();
        employee.Name.Should().Be(request.Name);
        employee.NationalNumber.Should().Be(request.NationalNumber);
    }

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
        
        var request = CreateEmployeeRequest(requestEmployee);

        // Act
        var response = await _userClient.Client.PostAsJsonAsync("/api/Employees", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateEmployee_WithSameData_DifferentUsers_ShouldSucceed()
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
        var request1 = CreateEmployeeRequest(baseData);

        var uniqueData2 = TestDataGenerator.Employee.Generate();
        var request2 = CreateEmployeeRequest(uniqueData2);

        // Act
        var response1 = await UserClient1.Client.PostAsJsonAsync("/api/Employees", request1);
        var response2 = await UserClient2.Client.PostAsJsonAsync("/api/Employees", request2);

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.Created);
        response2.StatusCode.Should().Be(HttpStatusCode.Created);
    }
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

        var request = CreateEmployeeRequest(requestEmployee);

        // Act
        var response = await _userClient.Client.PostAsJsonAsync("/api/Employees", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
    [Fact]
    public async Task CreateEmployee_ConcurrentCalls_WithSameData_ShouldAllowOnlyOne()
    {
        // Arrange
        var employeeData = TestDataGenerator.Employee
            .ForProperty(e=>e.CreatedBy, _userClient.User.Id)
            .WithValue(EmployeeFields.NationalNumber)
            .Generate();
        var request = CreateEmployeeRequest(employeeData);

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