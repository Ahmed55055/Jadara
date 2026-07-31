using Microsoft.AspNetCore.Http;
using Reward_Flow_v2.Employees.Data;
using RewardFlow_API.Employees.Common;
using System.Net.Http.Json;

namespace RewardFlow.IntegrationTests.Employees.Common;

public class ApiManager
{
    /// <summary>
    /// Creates an employee asynchronously using the provided HTTP client.
    /// </summary>
    /// <param name="employee">The employee data to create.</param>
    /// <param name="client">The HTTP client used to make the API request.</param>
    /// <returns>The created employee DTO if successful; otherwise, null.</returns>
    public static async Task<EmployeeDto?> CreateEmployee(Employee employee, HttpClient client)
    {
        var request = RequestCreator.CreateEmployeeRequest(employee);
        var createResponse = await client.PostAsJsonAsync("/api/Employees", request);
        createResponse.EnsureSuccessStatusCode();
        return await createResponse.Content.ReadFromJsonAsync<EmployeeDto>();
    }
}