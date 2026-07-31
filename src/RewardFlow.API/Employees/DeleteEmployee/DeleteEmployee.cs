using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Reward_Flow_v2.Common;
using Reward_Flow_v2.Common.EndpointValidation;
using Reward_Flow_v2.Employees.Data.Database;
using RewardFlow_API.Employees.Common;
using System.Security.Claims;

namespace Reward_Flow_v2.Employees.DeleteEmployee;

[Authorize(Roles = "User")]
public static partial class DeleteEmployee
{
    public static void MapDeleteEmployee(this IEndpointRouteBuilder app)
    {
        app.MapDelete(EmployeeApiPath.Delete, HandlerAsync)
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithTags(EmployeeApiPath.Tag);
    }

    private static async Task<IResult> HandlerAsync(int id, EmployeeDbContext dbContext, IEmployeeTokenService tokenService, IHttpContextAccessor httpContextAccessor, CancellationToken cancellationToken)
    {
        var userId = await httpContextAccessor.GetCurrentUserIntIdAsync(cancellationToken);

        if (userId == 0)
            return Results.Unauthorized();

        try
        {
            var employee = await dbContext.Employee
                .Where(e => e.EmployeeId == id && e.CreatedBy == userId)
                .FirstOrDefaultAsync(cancellationToken);

            if (employee == null)
                return Results.NotFound();

            await tokenService.DeleteTokensAsync(id, userId, cancellationToken);
            
            dbContext.Employee.Remove(employee);
            await dbContext.SaveChangesAsync(cancellationToken);
            
            return Results.NoContent();
        }
        catch (Exception)
        {
            return Results.InternalServerError();
        }
    }
}