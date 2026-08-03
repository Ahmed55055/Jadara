using Microsoft.EntityFrameworkCore;
using Reward_Flow_v2.Common;
using Reward_Flow_v2.Employees.Data.Database;
using RewardFlow_API.Employees.Common;

namespace Reward_Flow_v2.Employees.GetEmployeeByName;

public static partial class GetEmployeeByName
{
    public static void MapGetEmployeeByName(this IEndpointRouteBuilder app)
    {
        app.MapGet(EmployeeApiPath.GetByName, HandlerAsync)
            .RequireAuthorization()
            .Produces<EmployeeDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithTags(EmployeeApiPath.Tag);
    }

    private static async Task<IResult> HandlerAsync(string name, EmployeeDbContext dbContext, IHttpContextAccessor httpContextAccessor, CancellationToken cancellationToken)
    {
        var currentUserId = await httpContextAccessor.GetCurrentUserIntIdAsync(cancellationToken);
        
        if(currentUserId == 0)
            return Results.Unauthorized();

        try
        {
            var employee = await dbContext.Employee
                .Where(e => e.Name == name && e.CreatedBy == currentUserId)
                .FirstOrDefaultAsync(cancellationToken);

            return employee == null ? Results.NotFound() : Results.Ok(employee.ToDto());
        }
        catch (Exception)
        {
            return Results.InternalServerError();
        }
    }
}