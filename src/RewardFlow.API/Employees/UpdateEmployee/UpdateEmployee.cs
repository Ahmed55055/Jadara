using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Reward_Flow_v2.Common;
using Reward_Flow_v2.Common.EndpointValidation;
using Reward_Flow_v2.Employees.Data;
using Reward_Flow_v2.Employees.Data.Database;
using RewardFlow_API.Employees.Common;
using System.Security.Claims;

namespace Reward_Flow_v2.Employees.UpdateEmployee;

public static partial class UpdateEmployee
{
    public record Request
    {
        public Optional<string> Name { get; init; }
        public Optional<string?> NationalNumber { get; init; }
        public Optional<string?> AccountNumber { get; init; }
        public Optional<decimal?> Salary { get; init; }
        public Optional<int?> FacultyId { get; init; }
        public Optional<int?> DepartmentId { get; init; }
        public Optional<byte?> JobTitle { get; init; }
        public Optional<byte?> Status { get; init; }
    }
    public static void MapUpdateEmployee(this IEndpointRouteBuilder app)
    {
        app.MapPatch(EmployeeApiPath.Update, HandlerAsync)
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces<IEnumerable<FluentValidation.Results.ValidationFailure>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithTags(EmployeeApiPath.Tag)
            .Validation(new UpdateEmployeeRequestValidator());
    }

    private static async Task<IResult> HandlerAsync(int id, Request request, EmployeeDbContext dbContext, IEmployeeTokenService tokenService, IHttpContextAccessor httpContextAccessor, CancellationToken cancellationToken)
    {
        var currentUserId = await httpContextAccessor.GetCurrentUserIntIdAsync(cancellationToken);
        
        if(currentUserId == 0)
            return Results.Unauthorized();

        try
        {
            var employee = await dbContext.Employee
                .Where(e => e.Id == id && e.CreatedBy == currentUserId)
                .FirstOrDefaultAsync(cancellationToken);

            if (employee == null)
                return Results.NotFound();

            MapUpdateFieldsToEmployee(employee, request);

            await dbContext.SaveChangesAsync(cancellationToken);
            
            if (request.Name.HasValue)
            {
                await tokenService.UpdateTokensAsync(employee, currentUserId, cancellationToken); }
            
            return Results.NoContent();
        }
        catch (Exception)
        {
            return Results.InternalServerError();
        }
        
    }
    
    private static void MapUpdateFieldsToEmployee(Employee employee, Request request)
    {
        if (request.Name.HasValue) employee.Name = request.Name;
        if (request.NationalNumber.HasValue) employee.NationalNumber = request.NationalNumber;
        if (request.AccountNumber.HasValue) employee.AccountNumber = request.AccountNumber;
        if (request.Salary.HasValue) employee.Salary = request.Salary;
        if (request.FacultyId.HasValue) employee.FacultyId = request.FacultyId;
        if (request.DepartmentId.HasValue) employee.DepartmentId = request.DepartmentId;
        if (request.JobTitle.HasValue) employee.JobTitle = request.JobTitle;
        if (request.Status.HasValue) employee.Status = request.Status;
    }
}