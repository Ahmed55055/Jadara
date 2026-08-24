namespace RewardFlow_API.Employees.BulkInsertEmployees.Interfaces;

internal interface IBulkEmployeesImporter
{
    Task ExecuteAsync(Guid  batchId,Guid tenantId, CancellationToken cancellationToken = default);
}