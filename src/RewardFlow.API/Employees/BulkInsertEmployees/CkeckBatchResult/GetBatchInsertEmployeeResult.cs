using Microsoft.EntityFrameworkCore;
using Reward_Flow_v2.Common.EndpointValidation;
using Reward_Flow_v2.Employees.Data.Database;

namespace Reward_Flow_v2.Employees.BulkInsertEmployees.CkeckBatchResult;

public static class GetBatchInsertEmployeeResult
{
    public static void MapBatchInsertEmployeeResult(this IEndpointRouteBuilder app)
    {
        app.MapGet(EmployeeApiPath.BulkInsertResult, HandlerAsync)
            .RequireAuthorization()
            .Produces<BatchResult>()
            .Produces(StatusCodes.Status400BadRequest)
            .WithTags(EmployeeApiPath.Tag)
            .ValidateAccess();
    }

    private static async Task<IResult> HandlerAsync(Guid batchId, EmployeeDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var result = await dbContext.BulkImportResults
            .Where(r => r.BatchId == batchId)
            .GroupBy(_ => 1)
            .Select(g => new BatchResult(
                TotalRecords: g.Count(),
                TotalSucceeded: g.Count(r => r.ErrorTypeCode == null),
                Failed: g
                    .Where(r => r.ErrorTypeCode != null)
                    .Select(r => new FailedRecord(
                        r.Tracker,
                        r.ErrorTypeCode!,
                        r.ErrorMessage!
                    ))
                    .ToArray()
            ))
            .FirstOrDefaultAsync(cancellationToken);
        
        return result is null ? Results.NotFound() : Results.Ok(result);
    }
}

public record BatchResult(
    int TotalRecords,
    int TotalSucceeded,
    FailedRecord[] Failed
);

public record FailedRecord(Guid TrackerId, string Reason, string Message);