namespace RewardFlow_API.Employees.BulkInsertEmployees.Interfaces;

internal interface ITokenBackgroundJob
{
    Task GenerateBatchTokens(Guid batchId, Guid tenantId);
}