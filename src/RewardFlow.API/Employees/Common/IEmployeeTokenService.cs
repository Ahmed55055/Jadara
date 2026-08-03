using Reward_Flow_v2.Employees.Data;

namespace RewardFlow_API.Employees.Common;

public interface IEmployeeTokenService
{
    public IEnumerable<EmployeeNameToken> CreateTokens(IEnumerable<Employee> employees, int userId,
        CancellationToken cancellationToken = default);    
    public IEnumerable<EmployeeNameToken> CreateTokens(Employee employee, int userId,
        CancellationToken cancellationToken = default);
    Task CreateTokensAsync(Employee employee, int userId, CancellationToken cancellationToken = default);
    Task UpdateTokensAsync(Employee employee, int userId, CancellationToken cancellationToken = default);
    Task DeleteTokensAsync(int employeeId, int userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<int>> SearchEmployeesByNameAsync(string searchName, int userId, int limit = 10, CancellationToken cancellationToken = default);
    List<EmployeeNameToken> CreateTokens(string employeeName, int employeeId, int userId);
}