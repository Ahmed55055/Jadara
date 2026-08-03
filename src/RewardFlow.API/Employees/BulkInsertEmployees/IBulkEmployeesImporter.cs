namespace Reward_Flow_v2.Employees.BulkInsertEmployees;

internal interface IBulkEmployeesImporter
{
    Task ExecuteAsync(Guid  batchId,Guid tenantId, CancellationToken cancellationToken = default);
}