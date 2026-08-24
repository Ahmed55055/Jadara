namespace RewardFlow_API.Employees.BulkInsertEmployees.Interfaces;

internal interface IBulkInserter<in T> where T : class
{
    Task BulkInsertAsync(IEnumerable<T> employees, int userId, Guid tenantId);
}