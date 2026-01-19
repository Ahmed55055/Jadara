using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using RewardFlow.IntegrationTests.Infrastructure;
using Reward_Flow_v2.Employees.Common;
using Reward_Flow_v2.Employees.Data;
using Reward_Flow_v2.Employees.UpdateEmployee;
using RewardFlow.IntegrationTests.Employees.Common;
using Xunit;

namespace RewardFlow.IntegrationTests.Employees;

public class EmployeeFullLifecycleTests : BaseIntegrationTest
{
    private readonly JsonSerializerOptions _jsonOptions;

    public EmployeeFullLifecycleTests(TestWebApplicationFactory factory) : base(factory)
    {
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    [Fact]
    public async Task EmployeeOperations_FullWorkflow_ShouldWorkCorrectly()
    {
        // Test 1: Create Employee
        var employeeData = new Employee
        {
            Name = "John Doe",
            NationalNumber = "12345678901",
            AccountNumber = "ACC123456",
            Salary = 5000.0f,
            FacultyId = 1,
            DepartmentId = 1,
            JobTitle = (byte)1,
            Status = (byte)1
        };

        var createdEmployee = await ApiManager.CreateEmployee(employeeData, Client);
        createdEmployee.Should().NotBeNull();
        createdEmployee!.Id.Should().BeGreaterThan(0);
        createdEmployee.Name.Should().Be("John Doe");

        var employeeId = createdEmployee.Id;

        // Test 2: Get Employee by ID
        var getByIdResponse = await Client.GetAsync($"/api/Employees/{employeeId}");
        getByIdResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var retrievedEmployee = await getByIdResponse.Content.ReadFromJsonAsync<EmployeeDto>(_jsonOptions);
        retrievedEmployee.Should().NotBeNull();
        retrievedEmployee!.Id.Should().Be(employeeId);
        retrievedEmployee.Name.Should().Be("John Doe");

        // Test 3: Get Employee by National Number
        var getByNationalResponse = await Client.GetAsync($"/api/Employees/national/{employeeData.NationalNumber}");
        getByNationalResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var employeeByNational = await getByNationalResponse.Content.ReadFromJsonAsync<EmployeeDto>(_jsonOptions);
        employeeByNational.Should().NotBeNull();
        employeeByNational!.NationalNumber.Should().Be(employeeData.NationalNumber);

        // Test 4: Get Employee by Name
        var getByNameResponse = await Client.GetAsync($"/api/Employees/name/{Uri.EscapeDataString(employeeData.Name)}");
        getByNameResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Test 5: Update Employee
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

        var updateResponse = await Client.PatchAsJsonAsync($"/api/Employees/{employeeId}", updateRequest);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify update
        var updatedGetResponse = await Client.GetAsync($"/api/Employees/{employeeId}");
        var updatedEmployee = await updatedGetResponse.Content.ReadFromJsonAsync<EmployeeDto>(_jsonOptions);
        updatedEmployee!.Name.Should().Be("John Smith");
        updatedEmployee.Salary.Should().Be(6000.0f);

        // Test 6: Create additional employees for bulk operations
        var bulkEmployees = new[]
        {
            new Employee { Name = "Jane Doe", NationalNumber = "98765432101", AccountNumber = "ACC789", Salary = 4500.0f, FacultyId = 1, DepartmentId = 1, JobTitle = (byte)1, Status = (byte)1 },
            new Employee { Name = "Bob Wilson", NationalNumber = "11223344556", AccountNumber = "ACC456", Salary = 5500.0f, FacultyId = 1, DepartmentId = 1, JobTitle = (byte)1, Status = (byte)1 }
        };

        foreach (var emp in bulkEmployees)
        {
            await ApiManager.CreateEmployee(emp, Client);
        }

        // Test 7: Get All Employees
        var getAllResponse = await Client.GetAsync("/api/Employees");
        getAllResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var allEmployees = await getAllResponse.Content.ReadFromJsonAsync<List<EmployeeDto>>(_jsonOptions);
        allEmployees.Should().NotBeNull();
        allEmployees!.Count.Should().BeGreaterThanOrEqualTo(3);

        // Test 8: Search Employees by Name
        var searchResponse = await Client.GetAsync("/api/Employees/search?name=John");
        searchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var searchResults = await searchResponse.Content.ReadFromJsonAsync<List<EmployeeDto>>(_jsonOptions);
        searchResults.Should().NotBeNull();
        searchResults!.Should().HaveCountGreaterThanOrEqualTo(1);
        searchResults.Should().Contain(e => e.Name.Contains("John"));

        // Test 9: Bulk Insert Employees
        var bulkInsertEmployees = new[]
        {
            new Employee { Name = "Alice Johnson", NationalNumber = "55566677788", AccountNumber = "ACC999", Salary = 4800.0f, FacultyId = 1, DepartmentId = 1, JobTitle = (byte)1, Status = (byte)1 },
            new Employee { Name = "Charlie Brown", NationalNumber = "99988877766", AccountNumber = "ACC888", Salary = 5200.0f, FacultyId = 1, DepartmentId = 1, JobTitle = (byte)1, Status = (byte)1 }
        };

        var bulkInsertResponse = await Client.PostAsJsonAsync("/api/Employees/BulkInsert", bulkInsertEmployees);
        bulkInsertResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify bulk insert worked
        var finalGetAllResponse = await Client.GetAsync("/api/Employees");
        var finalAllEmployees = await finalGetAllResponse.Content.ReadFromJsonAsync<List<EmployeeDto>>(_jsonOptions);
        finalAllEmployees!.Count.Should().BeGreaterThanOrEqualTo(5);

        // Test 10: Delete Employee (should be last test)
        var deleteResponse = await Client.DeleteAsync($"/api/Employees/{employeeId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deletion
        var deletedGetResponse = await Client.GetAsync($"/api/Employees/{employeeId}");
        deletedGetResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}