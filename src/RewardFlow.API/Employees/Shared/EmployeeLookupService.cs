using Microsoft.EntityFrameworkCore;
using Reward_Flow_v2.Common.EmployeeLookup;
using Reward_Flow_v2.Employees.Data;
using Reward_Flow_v2.Employees.Data.Database;

namespace Reward_Flow_v2.Employees.Shared;

public class EmployeeLookupService : IEmployeeLookupService
{
    private readonly EmployeeDbContext _dbContext;

    public EmployeeLookupService(EmployeeDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<Employee?> GetEmployeesAsync(int employeeId)
    {
        var employee = await _dbContext.Employee
            .Where(e => e.EmployeeId == employeeId)
            .FirstOrDefaultAsync();

        return employee;    
    }

    public async Task<IEnumerable<Employee>> GetEmployeesAsync(IEnumerable<int> employeesIds)
    {
        var employees = await _dbContext.Employee
            .Where(e => employeesIds.Contains( e.EmployeeId) )
            .ToListAsync();

        return employees;    
    }
}