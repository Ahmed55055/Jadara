using Reward_Flow_v2.Employees.CreateEmployee;
using Reward_Flow_v2.Employees.Data;
using Reward_Flow_v2.Employees.UpdateEmployee;

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


    public UpdateEmployee.Request Update()
    {
        return new UpdateEmployee.Request
        {
            Name = employee.Name,
            NationalNumber = employee.NationalNumber,
            AccountNumber = employee.AccountNumber,
            Salary = employee.Salary,
            FacultyId = employee.FacultyId,
            DepartmentId = employee.DepartmentId,
            JobTitle = employee.JobTitle,
            Status = employee.Status
        };
    }
}