using Reward_Flow_v2.Employees.CreateEmployee;
using Reward_Flow_v2.Employees.Data;

namespace RewardFlow.IntegrationTests.Employees.Common;

public static class RequestCreator
{
    /// <summary>
    /// Creates a CreateEmployee.Request object from an Employee entity.
    /// This helper method maps employee data to the request format expected by the API endpoint.
    /// </summary>
    /// <param name="employee">The employee entity containing the data to map</param>
    /// <returns>A CreateEmployee.Request object ready for API submission</returns>
    public static CreateEmployee.Request CreateEmployeeRequest(Employee employee)
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