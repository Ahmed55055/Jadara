using Microsoft.EntityFrameworkCore;
using Reward_Flow_v2.Common;
using Reward_Flow_v2.Employees.Data.Database;
using RewardFlow_API.Employees.Common;

namespace Reward_Flow_v2.Employees.GetEmployeeById;

public static class GetEmployeeById
{
    public static void MapGetEmployeeById(this IEndpointRouteBuilder app)
    {
        app.MapGet(EmployeeApiPath.GetById, HandlerAsync)
            .RequireAuthorization()
            .Produces<EmployeeDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithTags(EmployeeApiPath.Tag);
    }

    private static async Task<IResult> HandlerAsync(int id, EmployeeDbContext dbContext, IHttpContextAccessor httpContextAccessor, CancellationToken cancellationToken)
    {
        var currentUserId = await httpContextAccessor.GetCurrentUserIntIdAsync(cancellationToken);
        
        if(currentUserId == 0)
            return Results.Unauthorized();

        try
        {
            var employee = await dbContext.Employee
                .Where(e => e.EmployeeId == id && e.CreatedBy == currentUserId)
                .FirstOrDefaultAsync(cancellationToken);

            return employee == null ? Results.NotFound() : Results.Ok(employee.ToDto());
        }
        catch (Exception)
        {
            return Results.InternalServerError();
        }
    }
}