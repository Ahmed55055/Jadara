using Bogus;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Reward_Flow_v2.Employees.Common;
using Reward_Flow_v2.Employees.CreateEmployee;
using Reward_Flow_v2.Employees.UpdateEmployee;
using RewardFlow.IntegrationTests.Infrastructure;
using Reward_Flow_v2.Employees.Data;
using RewardFlow.IntegrationTests.Employees.Common;
using RewardFlow.TestUtilities.DataGenerators;
using Xunit;

namespace RewardFlow.IntegrationTests.Employees;

/// <summary>
/// Integration tests for the Employee Full Lifecycle endpoint.
/// Tests various scenarios for employee operations including creation, retrieval, update, and deletion.
/// </summary>
[Collection("EmployeeTests")]
public class EmployeeFullLifecycleTests : IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;
    private readonly DbUtility _dbUtility;
    private UserClient _userClient;
    public EmployeeFullLifecycleTests(EmployeeTestFixture factory)
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
    public async Task EmployeeOperations_FullWorkflow_ShouldWorkCorrectly()
    {
        // Test 1: Create Employee
        var employeeId = await CreateEmployee();

        // Test 2: Get Employee by ID
        await GetEmployeeById(employeeId);

        // Test 3: Get Employee by National Number
        await GetEmployeeByNationalNumber(employeeId);

        // Test 4: Get Employee by Name
        await GetEmployeeByName(employeeId);

        // Test 5: Update Employee
        await UpdateEmployee(employeeId);

        // Test 6: Create additional employees for bulk operations
        await CreateAdditionalEmployees();

        // Test 7: Get All Employees
        await GetAllEmployees();

        // Test 8: Search Employees by Name
        await SearchEmployeesByName();

        // Test 9: Bulk Insert Employees
        await BulkInsertEmployees();

        // Test 10: Delete Employee (should be last test)
        await DeleteEmployee(employeeId);
    }

    private async Task<int> CreateEmployee()
    {
        var employeeData = TestDataGenerator.Employee.Generate();
        var request = RequestCreator.CreateEmployeeRequest(employeeData);

        var response = await _userClient.Client.PostAsJsonAsync("/api/Employees", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var createdEmployee = await response.Content.ReadFromJsonAsync<EmployeeDto>();
        createdEmployee.Should().NotBeNull();
        createdEmployee!.Id.Should().BeGreaterThan(0);
        createdEmployee.Name.Should().Be(employeeData.Name);

        return createdEmployee.Id;
    }

    private async Task GetEmployeeById(int employeeId)
    {
        var getByIdResponse = await _userClient.Client.GetAsync($"/api/Employees/{employeeId}");
        getByIdResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var retrievedEmployee = await getByIdResponse.Content.ReadFromJsonAsync<EmployeeDto>();
        retrievedEmployee.Should().NotBeNull();
        retrievedEmployee!.Id.Should().Be(employeeId);
    }

    private async Task GetEmployeeByNationalNumber(int employeeId)
    {
        var employee = await _dbUtility.Set<Employee>().FindAsync(employeeId);
        var getByNationalResponse = await _userClient.Client.GetAsync($"/api/Employees/national/{employee.NationalNumber}");
        getByNationalResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var employeeByNational = await getByNationalResponse.Content.ReadFromJsonAsync<EmployeeDto>();
        employeeByNational.Should().NotBeNull();
        employeeByNational!.NationalNumber.Should().Be(employee.NationalNumber);
    }

    private async Task GetEmployeeByName(int employeeId)
    {
        var employee = await _dbUtility.Set<Employee>().FindAsync(employeeId);
        var getByNameResponse = await _userClient.Client.GetAsync($"/api/Employees/name/{Uri.EscapeDataString(employee.Name)}");
        getByNameResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private async Task UpdateEmployee(int employeeId)
    {
        var updateRequest = new UpdateEmployee.Request
        {
            Name = "John Smith",
            NationalNumber = "12345678901",
            AccountNumber = "ACC654321",
            Salary = 6000.0f,
            FacultyId = 1,
            DepartmentId = 1,
            JobTitle = (byte)2,
            Status = (byte)1
        };

        var updateResponse = await _userClient.Client.PatchAsJsonAsync($"/api/Employees/{employeeId}", updateRequest);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify update
        var updatedGetResponse = await _userClient.Client.GetAsync($"/api/Employees/{employeeId}");
        var updatedEmployee = await updatedGetResponse.Content.ReadFromJsonAsync<EmployeeDto>();
        updatedEmployee!.Name.Should().Be("John Smith");
        updatedEmployee.Salary.Should().Be(6000.0f);
    }

    private async Task CreateAdditionalEmployees()
    {
        var bulkEmployees = TestDataGenerator.Employee.Generate(2);

        foreach (var emp in bulkEmployees)
        {
            var bulkRequest = RequestCreator.CreateEmployeeRequest(emp);
            await _userClient.Client.PostAsJsonAsync("/api/Employees", bulkRequest);
        }
    }

    private async Task GetAllEmployees()
    {
        var getAllResponse = await _userClient.Client.GetAsync("/api/Employees");
        getAllResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var allEmployees = await getAllResponse.Content.ReadFromJsonAsync<List<EmployeeDto>>();
        allEmployees.Should().NotBeNull();
        allEmployees!.Count.Should().BeGreaterThanOrEqualTo(3);
    }

    private async Task SearchEmployeesByName()
    {
        var searchResponse = await _userClient.Client.GetAsync("/api/Employees/search?name=John");
        searchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var searchResults = await searchResponse.Content.ReadFromJsonAsync<List<EmployeeDto>>();
        searchResults.Should().NotBeNull();
        searchResults!.Should().HaveCountGreaterThanOrEqualTo(1);
        searchResults.Should().Contain(e => e.Name.Contains("John"));
    }

    private async Task BulkInsertEmployees()
    {
        var bulkInsertEmployees = TestDataGenerator.Employee.Generate(2);

        var bulkInsertResponse = await _userClient.Client.PostAsJsonAsync("/api/Employees/BulkInsert", bulkInsertEmployees);
        bulkInsertResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify bulk insert worked
        var finalGetAllResponse = await _userClient.Client.GetAsync("/api/Employees");
        var finalAllEmployees = await finalGetAllResponse.Content.ReadFromJsonAsync<List<EmployeeDto>>();
        finalAllEmployees!.Count.Should().BeGreaterThanOrEqualTo(5);
    }

    private async Task DeleteEmployee(int employeeId)
    {
        var deleteResponse = await _userClient.Client.DeleteAsync($"/api/Employees/{employeeId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deletion
        var deletedGetResponse = await _userClient.Client.GetAsync($"/api/Employees/{employeeId}");
        deletedGetResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}