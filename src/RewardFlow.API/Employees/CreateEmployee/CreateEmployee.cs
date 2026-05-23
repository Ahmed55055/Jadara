using EntityFramework.Exceptions.Common;
using Microsoft.EntityFrameworkCore;
using Reward_Flow_v2.Common;
using Reward_Flow_v2.Common.EndpointValidation;
using Reward_Flow_v2.Employees.Common;
using Reward_Flow_v2.Employees.Data.Database;
using Reward_Flow_v2.Employees.Data;

namespace Reward_Flow_v2.Employees.CreateEmployee;

public static class CreateEmployee
{
    public record Request(
        string Name,
        string? NationalNumber,
        string? AccountNumber,
        decimal? Salary,
        int? FacultyId,
        int? DepartmentId,
        byte? JobTitle,
        byte? Status);

    public static void MapCreateEmployee(this IEndpointRouteBuilder app)
    {
        app.MapPost(EmployeeApiPath.Create, HandlerAsync)
            .RequireAuthorization()
            .Produces<Employee>(StatusCodes.Status201Created)
            .Produces<IEnumerable<FluentValidation.Results.ValidationFailure>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError)
            .Produces((StatusCodes.Status409Conflict))
            .WithTags(EmployeeApiPath.Tag)
            .Validation(new CreateEmployeeRequestValidator());
    }


    private static async Task<IResult> HandlerAsync(Request request, EmployeeDbContext dbContext,
        IEmployeeTokenService tokenService, IHttpContextAccessor httpContextAccessor,
        CancellationToken cancellationToken)
    {
        var currentUserId = await httpContextAccessor.GetCurrentUserIntIdAsync(cancellationToken);

        if (currentUserId == 0)
            return Results.Unauthorized();

        var employee = PrepareNewEmployeeObject(request, currentUserId);

        try
        {
            if (await dbContext.Employee.AnyAsync(e =>
                    e.CreatedBy == currentUserId &&
                    ((e.NationalNumberHash  != null && e.NationalNumberHash == employee.NationalNumberHash) ||
                     (e.AccountNumberHash != null && e.AccountNumberHash == employee.AccountNumberHash))))
                return Results.Conflict("Duplicate National Number or Account Number for this user");

            dbContext.Employee.Add(employee);
            await dbContext.SaveChangesAsync(cancellationToken);

            await tokenService.CreateTokensAsync(employee, currentUserId, cancellationToken);

            return Results.Created($"{EmployeeApiPath.GetById.Replace("{id}", employee.EmployeeId.ToString())}",
                employee);
        }
        catch (UniqueConstraintException)
        {
            return Results.Conflict("Duplicate National Number or Account Number for this user");
        }
        catch (Exception)
        {
            return Results.InternalServerError();
        }
    }

    private static Employee PrepareNewEmployeeObject(Request request, int currentUserId)
    {
        var emp = new Employee
        {
            Name = request.Name,
            NationalNumber = request.NationalNumber,
            AccountNumber = request.AccountNumber,
            Salary = request.Salary,
            FacultyId = request.FacultyId,
            DepartmentId = request.DepartmentId,
            JobTitle = request.JobTitle,
            Status = request.Status ?? 1,
            CreatedBy = currentUserId
        };

        return emp;
    }
}