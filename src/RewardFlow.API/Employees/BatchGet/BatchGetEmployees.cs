using Microsoft.EntityFrameworkCore;
using Reward_Flow_v2.Common;
using Reward_Flow_v2.Common.EndpointValidation;
using Reward_Flow_v2.Employees.Data.Database;
using RewardFlow_API.Employees.Common;

namespace Reward_Flow_v2.Employees.BatchGet;

public static class BatchGetEmployees
{
    public static void MapGetBatchEmployees(this IEndpointRouteBuilder app)
    {
        app.MapPost(EmployeeApiPath.GetBatch, HandlerAsync)
            .RequireAuthorization()
            .Produces<EmployeeDto[]>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithTags(EmployeeApiPath.Tag)
            .ValidateAccess();
    }

    private static async Task<IResult> HandlerAsync(int[] ids, EmployeeDbContext dbContext, CancellationToken cancellationToken)
    {
        try
        {
            var employees = await dbContext.Employee
                .Where(e=>ids.AsEnumerable().Contains(e.Id))
                .AsNoTracking()
                .Select(e=>e.ToDto())
                .ToArrayAsync(cancellationToken);

            return Results.Ok(employees);
        }
        catch (Exception)
        {
            return Results.InternalServerError();
        }
    }
}