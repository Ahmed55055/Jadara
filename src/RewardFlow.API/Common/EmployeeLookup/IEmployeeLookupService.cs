using Reward_Flow_v2.Employees.Data;

namespace Reward_Flow_v2.Common.EmployeeLookup;

public interface IEmployeeLookupService
{
    Task<Employee?> GetEmployeesAsync(int employeeId);
    Task<IEnumerable<Employee>> GetEmployeesAsync(IEnumerable<int> employeesIds);
}