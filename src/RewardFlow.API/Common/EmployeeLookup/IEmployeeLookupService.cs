using Reward_Flow_v2.Employees.Data;

namespace Reward_Flow_v2.Common.EmployeeLookup;

public interface IEmployeeLookupService
{
    Task<Employee?> GetEmployee(int employeeId);
    Task<IEnumerable<Employee>> GetEmployees(IEnumerable<int> employeesIds);
    Task<IEnumerable<EmployeeSalaryDto>> GetEmployeesSalaryById(IEnumerable<int> employeeIds);
}