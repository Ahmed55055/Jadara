using Hangfire;
using Microsoft.AspNetCore.Http;
using Reward_Flow_v2.Common;
using Reward_Flow_v2.Common.EndpointValidation;
using Reward_Flow_v2.Employees.Data;
using Reward_Flow_v2.Employees.Data.Database;
using RewardFlow_API.Common.Interface;
using System.Text.Json;

namespace Reward_Flow_v2.Employees.BulkInsertEmployees;

public static class BulkInsert_v2
{
    public static void MapBulkInsertEmployeeV2(this IEndpointRouteBuilder app)
    {
        app.MapPost(EmployeeApiPath.BulkInsertV2, HandlerAsync)
            .RequireAuthorization()
            .Produces<BulkImportBatch>(StatusCodes.Status202Accepted)
            .Produces(StatusCodes.Status400BadRequest)
            .WithTags(EmployeeApiPath.Tag)
            .WithMetadata(new { Version = "2.0" })
            .PreloadUser();
    }

    private static async Task<IResult> HandlerAsync(BulkInsert.Request request, IUserContext userContext,
        EmployeeDbContext dbContext, CancellationToken cancellationToken)
    {
        var batch = new BulkImportBatch
        {
            Id = Guid.NewGuid(),
            UserId = await userContext.GetUserIdAsync(),
            TenantId = userContext.GetTenantId(),
            TotalRecords = request.Employees.Count,
            RawPayloadJson = JsonSerializer.Serialize(request.Employees)
        };

        try
        {
            dbContext.Add(batch);
            await dbContext.SaveChangesAsync(cancellationToken);

            var employeesJobId =
                BackgroundJob.Enqueue<IBulkEmployeesImporter>(job =>
                    job.ExecuteAsync(batch.Id, userContext.GetTenantId()));
            
            BackgroundJob.ContinueJobWith<ITokenBackgroundJob>(employeesJobId,
                job => job.GenerateBatchTokens(batch.Id, userContext.GetTenantId()));

            return Results.Accepted(value: batch.Id);
        }
        catch (Exception e)
        {
            return Results.InternalServerError(new { error = "Failed to queue bulk insert", details = e.Message });
        }
    }
}