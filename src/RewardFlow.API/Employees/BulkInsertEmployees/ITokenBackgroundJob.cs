using System.Collections.Concurrent;

namespace Reward_Flow_v2.Employees.BulkInsertEmployees;

internal interface ITokenBackgroundJob
{
    Task GenerateBatchTokens(Guid batchId, Guid tenantId);
}