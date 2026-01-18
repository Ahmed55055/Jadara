using FluentAssertions;
using Reward_Flow_v2.Employees.Common;
using Reward_Flow_v2.Employees.CreateEmployee;
using Reward_Flow_v2.Employees.Data;
using RewardFlow.IntegrationTests.Employees.Common;
using RewardFlow.IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace RewardFlow.IntegrationTests.Employees;

[Collection("EmployeeTests")]
public class EmployeeSearchTests : IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;
    private readonly DbUtility _dbUtility;
    private UserClient _userClient;

    public EmployeeSearchTests(EmployeeTestFixture factory)
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

    [Theory]
    [InlineData("محمد", "محمدأحمد", "أحمد محمد", "محمد حسن", "محمد علي")]
    [InlineData("محمذ", "محمد أحمد", "أحمد محمد", "محمد حسن", "محمد علي")]
    [InlineData("عبدالرحمنعلي","عبد الرحمن علي محمد","عبد الرحمن على عزيز","عبد الله محمد على")]
    [InlineData("John Doe", "JohnDoe", "John MichaelDoe", "Johnathan Doe", "JohnnyDoe")]
    [InlineData("Jhondo", "John Doe", "Johnathan Doe", "JohnnyDoe", "Jonathan Davis")]
    public async Task SearchEmployeesByName_WithValidName_ShouldReturnMatchingEmployees(string searchName,
        params string[] employeeNames)
    {
        // Arrange
        var employees = new List<Employee>();
        foreach (var name in employeeNames)
        {
            var employee = TestDataGenerator.Employee
                .ForProperty(e => e.CreatedBy, _userClient.User.Id)
                .ForProperty(e => e.Name, name)
                .Generate();
            employees.Add(employee);
        }

        foreach (var employee in employees)
        {
            var request = RequestCreator.CreateEmployeeRequest(employee);
            var createResponse = await _userClient.Client.PostAsJsonAsync("/api/Employees", request);
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        // Act
        var response = await _userClient.Client.GetAsync($"/api/Employees/search?name={searchName}&limit=10");

        // Assert
        await AssertSearchSuccess(response, employees);
    }

    [Fact]
    public async Task SearchEmployeesByName_WithNoMatches_ShouldReturnEmptyList()
    {
        // Arrange
        var employeeData = TestDataGenerator.Employee
            .ForProperty(e => e.CreatedBy, _userClient.User.Id)
            .Generate();
        var request = RequestCreator.CreateEmployeeRequest(employeeData);
        var createResponse = await _userClient.Client.PostAsJsonAsync("/api/Employees", request);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Act
        var response = await _userClient.Client.GetAsync("/api/Employees/search?name=NonExistent&limit=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var employees = await response.Content.ReadFromJsonAsync<List<EmployeeDto>>();
        employees.Should().NotBeNull();
        employees!.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchEmployeesByName_WithDifferentLimitValues_ShouldRespectLimit()
    {
        // Arrange
        var employees = TestDataGenerator.Employee
            .ForProperty(e => e.CreatedBy, _userClient.User.Id)
            .RuleFor(e => e.Name, f => "Test " + f.Name.LastName())
            .Generate(15);

        foreach (var employee in employees)
        {
            var request = RequestCreator.CreateEmployeeRequest(employee);
            var createResponse = await _userClient.Client.PostAsJsonAsync("/api/Employees", request);
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        // Act
        var response = await _userClient.Client.GetAsync("/api/Employees/search?name=Test&limit=5");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var employeesResponse = await response.Content.ReadFromJsonAsync<List<Employee>>();
        employeesResponse.Should().NotBeNull();
        employeesResponse!.Count.Should().BeLessThanOrEqualTo(5);
    }

    [Fact]
    public async Task SearchEmployeesByName_WithEmptyParameters_ShouldReturnFirstPage()
    {
        // Arrange
        var employees = TestDataGenerator.Employee
            .ForProperty(e => e.CreatedBy, _userClient.User.Id)
            .Generate(25);

        foreach (var employee in employees)
        {
            var request = RequestCreator.CreateEmployeeRequest(employee);
            var createResponse = await _userClient.Client.PostAsJsonAsync("/api/Employees", request);
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        // Act
        var response = await _userClient.Client.GetAsync("/api/Employees/search?name=&limit=");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var employeesResponse = await response.Content.ReadFromJsonAsync<List<EmployeeDto>>();
        employeesResponse.Should().NotBeNull();
        employeesResponse!.Count.Should().BeGreaterThan(0);
    }

    /// <summary>
    /// Asserts that an employee search request succeeded and returned the expected employees
    /// in the correct order of relevance.
    /// </summary>
    /// <param name="response">
    /// The HTTP response returned from the search endpoint.
    /// </param>
    /// <param name="expectedEmployees">
    /// The list of expected employees, ordered by relevance.
    /// The order of this collection must match the order of employees returned in the response.
    /// </param>
    private async Task AssertSearchSuccess(HttpResponseMessage response, List<Employee> expectedEmployees)
    {
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var employeesResponse = await response.Content.ReadFromJsonAsync<List<EmployeeDto>>();
        employeesResponse.Should().NotBeNull()
            .And.HaveSameCount(expectedEmployees);

        employeesResponse.Should().OnlyHaveUniqueItems(e => e.Id);

        employeesResponse.Select(e => e.Id)
            .Should()
            .Equal(expectedEmployees.Select(e => e.EmployeeId));
    }
}