using Reward_Flow_v2.Employees.CreateEmployee;
using Reward_Flow_v2.Employees.Data;

namespace RewardFlow.TestUtilities.RequestExtentionHandler.Employees;

public class EmployeeRequest(Employee employee)
{
    public CreateEmployee.Request Create()
    {
        return new CreateEmployee.Request(
            Name: employee.Name,
            NationalNumber : employee.NationalNumber,
            AccountNumber : employee.AccountNumber,
            Salary : employee.Salary ,
            FacultyId : employee.FacultyId,
            DepartmentId : employee.DepartmentId,
            JobTitle : employee.JobTitle,
            Status : employee.Status 
        );
    }
    
    
}