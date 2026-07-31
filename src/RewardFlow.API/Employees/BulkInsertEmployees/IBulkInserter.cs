namespace Reward_Flow_v2.Employees.BulkInsertEmployees;

internal interface IBulkInserter<in T> where T : class
{
    Task BulkInsertAsync(IEnumerable<T> employees, int userId, Guid tenantId);
}