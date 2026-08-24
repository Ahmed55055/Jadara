using Microsoft.EntityFrameworkCore;
using Reward_Flow_v2.Common.EndpointValidation;
using Reward_Flow_v2.Employees.Data;
using Reward_Flow_v2.Employees.Data.Database;

namespace Reward_Flow_v2.Employees.BulkInsertEmployees.CheckEmployeesConflicts;

public static class CheckEmployeesConflicts
{
    public static void MapEmployeeConflictCheck(this IEndpointRouteBuilder app)
    {
        app.MapPost(EmployeeApiPath.ConflictCheck, HandlerAsync)
            .RequireAuthorization()
            .Produces<EmployeesConflictCheckResponse[]>()
            .Produces(StatusCodes.Status400BadRequest)
            .WithTags(EmployeeApiPath.Tag)
            .ValidateAccess();
    }

    private static async Task<IResult> HandlerAsync(List<EmployeesConflictCheckRequest> request, EmployeeDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var nationalHashes = request
            .Select(x => Employee.HashField(x.NationalNumber))
            .ToArray();

        var accountHashes = request
            .Select(x => Employee.HashField(x.AccountNumber))
            .ToArray();

        var conflicts = await dbContext.Employee
            .AsNoTracking()
            .Where(e =>
                (e.NationalNumberHash != null && nationalHashes.Contains(e.NationalNumberHash)) ||
                (e.AccountNumberHash != null && accountHashes.Contains(e.AccountNumberHash)))
            .Select(e => new
            {
                e.Id,
                e.Name,
                e.NationalNumberHash,
                e.AccountNumberHash
            })
            .ToListAsync(cancellationToken);

        var result = request
            .SelectMany(item =>
            {
                var nationalHash = Employee.HashField(item.NationalNumber);
                var accountHash = Employee.HashField(item.AccountNumber);

                return conflicts
                    .Where(e =>
                        e.NationalNumberHash == nationalHash ||
                        e.AccountNumberHash == accountHash)
                    .Select(e => new EmployeesConflictCheckResponse(
                        item.Tracker,
                        e.Id,
                        e.Name,
                        new[]
                            {
                                e.NationalNumberHash == nationalHash ? "NationalNumber" : null,
                                e.AccountNumberHash == accountHash ? "AccountNumber" : null
                            }
                            .Where(x => x is not null)
                            .Select(x => x!)
                            .ToArray()));
            })
            .ToArray();

        return Results.Ok(result);
    }
}

public record EmployeesConflictCheckRequest(Guid Tracker, string NationalNumber, string AccountNumber);
public record EmployeesConflictCheckResponse(Guid Tracker,int EmployeeId, string Name, string[] ConflictFields);


