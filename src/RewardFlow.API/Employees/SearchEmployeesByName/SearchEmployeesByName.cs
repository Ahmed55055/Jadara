using Microsoft.EntityFrameworkCore;
using Reward_Flow_v2.Common;
using Reward_Flow_v2.Employees.Data.Database;
using RewardFlow_API.Employees.Common;
using System.Linq;
namespace Reward_Flow_v2.Employees.SearchEmployeesByName;

public static class SearchEmployeesByName
{
    public static void MapSearchEmployeesByName(this IEndpointRouteBuilder app)
    {
        app.MapGet(EmployeeApiPath.SearchByName, HandlerAsync)
            .RequireAuthorization()
            .Produces<IEnumerable<EmployeeDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithTags(EmployeeApiPath.Tag);
    }

    private static async Task<IResult> HandlerAsync(string name, int limit, EmployeeDbContext dbContext, IEmployeeTokenService tokenService, IHttpContextAccessor httpContextAccessor, CancellationToken cancellationToken)
    {
        var currentUserId = await httpContextAccessor.GetCurrentUserIntIdAsync(cancellationToken);
        
        if(currentUserId == 0)
            return Results.Unauthorized();

        try
        {
            var employeeIds = await tokenService.SearchEmployeesByNameAsync(name, currentUserId, limit, cancellationToken);

            var ids = employeeIds.ToList();

            var employees = await dbContext.Employee
                .Where(e => employeeIds.Contains(e.EmployeeId) && e.CreatedBy == currentUserId)
                .ToListAsync(cancellationToken);

            var orderedEmployees = employees.OrderBy(e => ids.IndexOf(e.EmployeeId)).Select(e => e.ToDto());

            return Results.Ok(orderedEmployees);
        }
        catch (Exception)
        {
            return Results.InternalServerError();
        }
    }
}