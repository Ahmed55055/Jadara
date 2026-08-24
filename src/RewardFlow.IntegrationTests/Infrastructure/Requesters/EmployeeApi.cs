using FluentAssertions;
using Reward_Flow_v2.Employees;
using Reward_Flow_v2.Employees.BulkInsertEmployees;
using Reward_Flow_v2.Employees.BulkInsertEmployees.CkeckBatchResult;
using Reward_Flow_v2.Employees.CreateEmployee;
using Reward_Flow_v2.Employees.Data;
using RewardFlow_API.Employees.Common;
using System.Net;
using System.Net.Http.Json;

namespace RewardFlow.IntegrationTests.Infrastructure.Requesters;

public class EmployeeApi(UserClient userClient)
{
    /// <summary>
    /// Helper to post the bulk request and extract the returned Batch ID
    /// </summary>
    public async Task<Guid> ImportAsync(IEnumerable<Employee> employees,
        HttpStatusCode expectedStatusCode = HttpStatusCode.Accepted)
    {
        var bulkRequest = new BulkRequest(employees.Select(MapToBatchEmployee).ToList());
        var response = await userClient.Client.PostAsJsonAsync(EmployeeApiPath.BulkInsert, bulkRequest);
        
        response.StatusCode.Should().Be(expectedStatusCode);

        var batchId = await response.Content.ReadFromJsonAsync<Guid>();
        batchId.Should().NotBeEmpty().Should().NotBe(Guid.Empty);

        return batchId;
        
        static BatchEmployee MapToBatchEmployee(Employee employee)
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
    }

    public async Task<BatchResult?> GetBatchResult(Guid batchId, HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
    {
        var response = await userClient.Client.GetAsync(
            EmployeeApiPath.BulkInsertResult.Replace("{batchId}", batchId.ToString()));

        response.StatusCode.Should().Be(expectedStatusCode);

        if (response.StatusCode != HttpStatusCode.OK)
            return null;

        var result = await response.Content.ReadFromJsonAsync<BatchResult>();

        result.Should().NotBeNull();

        return result;
    }

    public async Task<EmployeeDto?> CreateEmployee(Employee employee, HttpStatusCode expectedStatusCode = HttpStatusCode.Created)
    {
        var request = CreateEmployeeRequest(employee);
        var createResponse = await userClient.Client.PostAsJsonAsync(EmployeeApiPath.Create, request);
        createResponse.StatusCode.Should().Be(expectedStatusCode);
        return await createResponse.Content.ReadFromJsonAsync<EmployeeDto>();
        
        static CreateEmployee.Request CreateEmployeeRequest(Employee employee)
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
}