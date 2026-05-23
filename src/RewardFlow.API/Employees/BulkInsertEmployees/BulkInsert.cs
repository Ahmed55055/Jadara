using EntityFramework.Exceptions.Common;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Reward_Flow_v2.Common;
using Reward_Flow_v2.Common.EndpointValidation;
using Reward_Flow_v2.Employees.Common;
using Reward_Flow_v2.Employees.CreateEmployee;
using Reward_Flow_v2.Employees.Data;
using Reward_Flow_v2.Employees.Data.Database;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Reward_Flow_v2.Employees.BulkInsertEmployees;

public static class BulkInsert
{
    public record emp(
        string Name,
        string? NationalNumber = null,
        string? AccountNumber = null,
        decimal?   Salary = null);
    public record Request(List<emp> Employees);
    public record Response(int Success, List<int> FailsIndexes);

    public static void MapBulkInsertEmployee(this IEndpointRouteBuilder app)
    {
        app.MapPost(EmployeeApiPath.BulkInsert, HandlerAsync)
            .RequireAuthorization()
            .Produces<Response>(StatusCodes.Status202Accepted)
            .Produces(StatusCodes.Status400BadRequest)
            .WithTags(EmployeeApiPath.Tag)
            .Validation(new BulkInsertEmployeeRequestValidator());
    }


    private static async Task<IResult> HandlerAsync(Request request, EmployeeDbContext dbContext, IEmployeeTokenService tokenService, IHttpContextAccessor httpContextAccessor, CancellationToken cancellationToken)
    {
        var CurrentUserId = await httpContextAccessor.GetCurrentUserIntIdAsync();

        if (CurrentUserId <= 0)
            Results.BadRequest();

        var EmployeeList =
            request.Employees
            .Select(e => PrepareEmployee(e, CurrentUserId))
            .ToList();

        var Success = 0;
        var FailIndexes = new List<int>();
        var IsExceptionThrown = false;

        for (int i = EmployeeList.Count - 1; i >= 0; i--)
        {
            var employee = EmployeeList[i];
            employee.CleanUp();

            if (string.IsNullOrWhiteSpace(employee.Name))
            {
                FailIndexes.Add(i);
                continue;
            }

            try
            {
                dbContext.Add(EmployeeList[i]);
                await dbContext.SaveChangesAsync();

                await tokenService.CreateTokensAsync(EmployeeList[i], CurrentUserId);

                Success += 1;
            }
            catch (UniqueConstraintException)
            {
                FailIndexes.Add(i);
            }
            catch (Exception)
            {
                IsExceptionThrown = true;
                FailIndexes.Add(i);
            }
        }

        var response = new Response(Success, FailIndexes);
        if (Success > 0)
            return Results.Accepted(value: response);

        // No Entities is added and throwed exception at addtion
        if (IsExceptionThrown)
            return Results.InternalServerError();

        // No Exception Happened And No Insertions, Possible Reasons
        // 1. Data Was Corrupted
        // 2. Wrong Data And Cleaned Up
        // 3. Conflict with unique constraint
        return Results.BadRequest(response);
    }

    private static Employee PrepareEmployee(emp employee, int currentUserId)
    {
        return new Employee
        {
            Name = employee.Name,
            NationalNumber = employee.NationalNumber,
            AccountNumber = employee.AccountNumber,
            Salary = employee.Salary,
            CreatedBy = currentUserId,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static void CleanUp(this Employee employee)
    {
        // Name cleanup
        if (!string.IsNullOrWhiteSpace(employee.Name))
        {
            employee.Name = Regex.Replace(employee.Name, @"[^a-zA-Z\u0600-\u06FF\s]", "");
            employee.Name = Regex.Replace(employee.Name, @"\s+", " ").Trim();
        }

        // National number cleanup
        if (!string.IsNullOrWhiteSpace(employee.NationalNumber))
        {
            employee.NationalNumber = Regex.Replace(employee.NationalNumber, @"[^0-9]", "").Trim();
        }
    }
}
