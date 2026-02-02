using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RewardFlow.IntegrationTests.Infrastructure;
using Reward_Flow_v2.Employees.Data;
using RewardFlow.TestUtilities.DataGenerators;
using RewardFlow.TestUtilities.DataGenerators.Fakers.Employees;
using Xunit;

namespace RewardFlow.IntegrationTests.Employees;

[Collection("EmployeeTests")]
public class GetEmployeeTests : IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;
    private readonly DbUtility _dbUtility;
    private UserClient _userClient;

    public GetEmployeeTests(EmployeeTestFixture factory)
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
    public async Task GetEmployeeById_WithValidId_ShouldReturnEmployee()
    {
        // Arrange
        var employeeData = TestDataGenerator.Employee
            .ForProperty(e => e.CreatedBy, _userClient.User.Id)
            .Generate();
        await _dbUtility.InsertAsync(employeeData);

        // Act
        var response = await _userClient.Client.GetAsync($"/api/Employees/{employeeData.EmployeeId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var employee = await response.Content.ReadFromJsonAsync<Employee>();
        employee.Should().NotBeNull();
        employee!.EmployeeId.Should().Be(employeeData.EmployeeId);
        employee.Name.Should().Be(employeeData.Name);
        employee.CreatedBy.Should().Be(_userClient.User.Id);
    }

    [Fact]
    public async Task GetEmployeeById_WithInvalidId_ShouldReturnNotFound()
    {
        // Act
        var response = await _userClient.Client.GetAsync("/api/Employees/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetEmployeeByNationalNumber_WithValidNumber_ShouldReturnEmployee()
    {
        // Arrange
        var employeeData = TestDataGenerator.Employee
            .ForProperty(e => e.CreatedBy, _userClient.User.Id)
            .WithValue(EmployeeFields.NationalNumber)
            .Generate();
        await _dbUtility.InsertAsync(employeeData);

        // Act
        var response = await _userClient.Client.GetAsync($"/api/Employees/national/{employeeData.NationalNumber}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var employee = await response.Content.ReadFromJsonAsync<Employee>();
        employee.Should().NotBeNull();
        employee!.NationalNumber.Should().Be(employeeData.NationalNumber);
        employee.CreatedBy.Should().Be(_userClient.User.Id);
    }

    // TODO: Needs thoughtfully decision on how to handle this case or if it should be handled at all, and remove the functionality
    /*[Fact]
    public async Task GetEmployeeByName_WithValidName_ShouldReturnEmployee()
    {
        // Arrange
        var employeeData = TestDataGenerator.Employee
            .ForProperty(e => e.CreatedBy, _userClient.User.Id)
            .Generate();
        await _dbUtility.InsertAsync(employeeData);

        // Act
        var response = await _userClient.Client.GetAsync($"/api/Employees/name/{Uri.EscapeDataString(employeeData.Name)}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var employee = await response.Content.ReadFromJsonAsync<Employee>();
        employee.Should().NotBeNull();
        employee!.Name.Should().Be(employeeData.Name);
        employee.CreatedBy.Should().Be(_userClient.User.Id);
    }*/

    [Fact]
    public async Task GetAllEmployees_ShouldReturnEmployeeList()
    {
        // Arrange
        var employees = TestDataGenerator.Employee
            .ForProperty(e => e.CreatedBy, _userClient.User.Id)
            .Generate(2);
        await _dbUtility.InsertRangeAsync(employees);

        // Act
        var response = await _userClient.Client.GetAsync("/api/Employees?limit=30");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var employeesResponse = await response.Content.ReadFromJsonAsync<List<Employee>>();
        employeesResponse.Should().NotBeNull();
        employeesResponse!.Count.Should().BeGreaterThanOrEqualTo(2);
        employeesResponse.Should().OnlyContain(e => e.CreatedBy == _userClient.User.Id);
    }
}