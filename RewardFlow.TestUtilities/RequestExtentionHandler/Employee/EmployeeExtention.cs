using Reward_Flow_v2.Employees.BulkInsertEmployees;
using Reward_Flow_v2.Employees.Data;

namespace RewardFlow.TestUtilities.RequestExtentionHandler.Employees;

public static class EmployeeExtention
{
    public static EmployeeRequest ToRequest(this Employee employee)
    {
        return new EmployeeRequest(employee);
    }

    public static EmployeeBulkRequest ToRequest(this IEnumerable<Employee> employees)
    {
        return new EmployeeBulkRequest(employees);
    }
}

public struct EmployeeBulkRequest
{
    public IEnumerable<Employee> Employees { get; set; }

    public EmployeeBulkRequest(IEnumerable<Employee> employees)
    {
        Employees = employees;
    }

    public BulkInsert.Request BulkInsert()
    {
        var emps = Employees
            .Select(e => new BulkInsert.emp(Guid.NewGuid(), e.Name, e.NationalNumber, e.AccountNumber, e.Salary));
        
        return new BulkInsert.Request(emps.ToList());
    }
}