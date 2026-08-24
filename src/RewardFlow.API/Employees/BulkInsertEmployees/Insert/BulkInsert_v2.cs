using Hangfire;
using Reward_Flow_v2.Common.EndpointValidation;
using Reward_Flow_v2.Employees;
using Reward_Flow_v2.Employees.BulkInsertEmployees;
using Reward_Flow_v2.Employees.Data;
using Reward_Flow_v2.Employees.Data.Database;
using RewardFlow_API.Common.Interface;
using RewardFlow_API.Employees.BulkInsertEmployees.Interfaces;
using System.Text.Json;

namespace RewardFlow_API.Employees.BulkInsertEmployees.Insert;

public static class BulkInsert_v2
{
    public static void MapBulkInsertEmployeeV2(this IEndpointRouteBuilder app)
    {
        app.MapPost(EmployeeApiPath.BulkInsert, HandlerAsync)
            .RequireAuthorization()
            .Produces<BulkImportBatch>(StatusCodes.Status202Accepted)
            .Produces(StatusCodes.Status400BadRequest)
            .WithTags(EmployeeApiPath.Tag)
            .PreloadUser();
    }

    private static async Task<IResult> HandlerAsync(BulkRequest request, IUserContext userContext,
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