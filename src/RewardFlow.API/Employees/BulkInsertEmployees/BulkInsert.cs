using EntityFramework.Exceptions.Common;
using FluentResults;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Reward_Flow_v2.Common;
using Reward_Flow_v2.Common.EndpointValidation;
using Reward_Flow_v2.Common.Enums;
using Reward_Flow_v2.Employees.CreateEmployee;
using Reward_Flow_v2.Employees.Data;
using Reward_Flow_v2.Employees.Data.Database;
using RewardFlow_API.Employees.Common;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Net;
using System.Text.RegularExpressions;

namespace Reward_Flow_v2.Employees.BulkInsertEmployees;

public static class BulkInsert
{

    public record Request(List<BatchEmployee> Employees);

    public record Response(BatchSummary Summary, SuccessfulRecord[] InsertedRecords, BulkError[] Errors);

    public record BatchSummary(int TotalRecords, int SuccessfulRecords, int FailedRecords);

    public record SuccessfulRecord(Guid Tracker, int EmployeeId, string Name);

    public record BulkError(Guid Tracker, ErrorTypes ErrorStatusCode, string Message);

    public enum ErrorTypes
    {
        InvalidName,
        DuplicateNationalNumber,
        DuplicateAccountNumber,
        DatabaseConflict,
        Unexpected
    }

    public static void MapBulkInsertEmployee(this IEndpointRouteBuilder app)
    {
        app.MapPost(EmployeeApiPath.BulkInsert, HandlerAsync)
            .RequireAuthorization()
            .Produces<Response>(StatusCodes.Status202Accepted)
            .Produces(StatusCodes.Status400BadRequest)
            .WithTags(EmployeeApiPath.Tag)
            .WithMetadata(new {Version = "1.0"})
            .Validation(new BulkInsertEmployeeRequestValidator())
            .PreloadUser();
    }

    private static async Task<IResult> HandlerAsync(Request request, EmployeeDbContext dbContext,
        ScopedUserContext scopedUserContext, IEmployeeTokenService tokenService, CancellationToken cancellationToken)
    {
        var userContext = scopedUserContext.User;
        var totalRecords = request.Employees.Count;
        var successfulRecords = new List<SuccessfulRecord>();
        var errors = new List<BulkError>();

        Dictionary<Guid, Employee> entitiesToInsert =
            ProcessDistinctEmployees(request, userContext, errors, tokenService);

        Dictionary<string, Guid> employeesNationalMap = entitiesToInsert
            .Where(kvp => !string.IsNullOrEmpty(kvp.Value.NationalNumber))
            .ToDictionary(
                kvp => kvp.Value.NationalNumber!,
                kvp => kvp.Key,
                StringComparer.OrdinalIgnoreCase);

        Dictionary<string, Guid> employeesAccountMap = entitiesToInsert
            .Where(kvp => !string.IsNullOrEmpty(kvp.Value.AccountNumber))
            .ToDictionary(
                kvp => kvp.Value.AccountNumber!,
                kvp => kvp.Key, // The Tracker Guid
                StringComparer.OrdinalIgnoreCase
            );

        var dbConflicts = await DbConflicts(dbContext, entitiesToInsert.Values, cancellationToken);

        RemoveDbConflictedEntities(dbConflicts, entitiesToInsert, employeesNationalMap, employeesAccountMap, errors);

        // 3. Database Bulk Insertion
        if (entitiesToInsert.Count > 0)
        {
            try
            {
                await dbContext.Employee.AddRangeAsync(entitiesToInsert.Values, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (Exception)
            {
                return Results.InternalServerError();
            }

            // Populate successful records
            successfulRecords.AddRange(from kvp in entitiesToInsert
                where kvp.Value.EmployeeId > 0
                select new SuccessfulRecord(kvp.Key, kvp.Value.EmployeeId, kvp.Value.Name));

            errors.AddRange(from kvp in entitiesToInsert
                where kvp.Value.EmployeeId == 0
                select new BulkError(kvp.Key, ErrorTypes.DatabaseConflict, "Failed to insert into database."));
        }

        var summary = new BatchSummary(
            TotalRecords: totalRecords,
            SuccessfulRecords: successfulRecords.Count,
            FailedRecords: errors.Count
        );

        var response = new Response(summary, [.. successfulRecords], [.. errors]);

        return Results.Accepted(value: response);
    }

    private static void RemoveDbConflictedEntities(
        List<(int EmployeeId, string? NationalNumber, string? AccountNumber)> dbConflicts,
        Dictionary<Guid, Employee> entitiesToInsertDic, Dictionary<string, Guid> employeesNationalMap,
        Dictionary<string, Guid> employeesAccountMap, List<BulkError> errors)
    {
        if (dbConflicts == null || dbConflicts.Count == 0)
        {
            return;
        }

        foreach (var conflict in dbConflicts)
        {
            bool isNationalConflict = employeesNationalMap.TryGetValue(conflict.NationalNumber!, out var trackerId);
            bool isAccountConflict = !isNationalConflict &&
                                     employeesAccountMap.TryGetValue(conflict.AccountNumber!, out trackerId);

            if (!isNationalConflict && !isAccountConflict)
                continue;

            entitiesToInsertDic.Remove(trackerId);

            string message = isNationalConflict
                ? "An employee with the same national number already exists in the database."
                : "An employee with the same account number already exists in the database.";

            errors.Add(new BulkError(trackerId, ErrorTypes.DatabaseConflict, message));
        }
    }

    private static Dictionary<Guid, Employee> ProcessDistinctEmployees(
        Request request, ScopedUserContextDto scopedUserContextDto, List<BulkError> errors, IEmployeeTokenService tokenService)
    {
        var entitiesToInsertDic = new Dictionary<Guid, Employee>();
        var seenNationalHashes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var seenAccountHashes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var rawEntity in request.Employees)
        {
            Guid trackerId = rawEntity.Tracker == Guid.Empty ? Guid.NewGuid() : rawEntity.Tracker;

            // Map and validate name via Domain Factory rules
            var entity = PrepareEmployee(rawEntity, scopedUserContextDto.Id, tokenService);

            // If creation failed, it means the name was empty or too short after regex cleaning
            if (entity == null)
            {
                errors.Add(new BulkError(trackerId, ErrorTypes.InvalidName,
                    "Name is invalid or too short after cleanup."));
                continue;
            }

            // 1. Validation: Check for duplicate national numbers
            if (!string.IsNullOrEmpty(entity.NationalNumber))
            {
                if (seenNationalHashes.Contains(entity.NationalNumber))
                {
                    errors.Add(new BulkError(trackerId, ErrorTypes.DuplicateNationalNumber,
                        "Duplicate national number detected within the request."));
                    continue;
                }

                seenNationalHashes.Add(entity.NationalNumber);
            }

            // 2. Validation: Check for duplicate account numbers
            if (!string.IsNullOrEmpty(entity.AccountNumber))
            {
                if (seenAccountHashes.Contains(entity.AccountNumber))
                {
                    errors.Add(new BulkError(trackerId, ErrorTypes.DuplicateAccountNumber,
                        "Duplicate account number detected within the request."));
                    continue;
                }

                seenAccountHashes.Add(entity.AccountNumber);
            }

            entitiesToInsertDic.Add(trackerId, entity);
        }

        return entitiesToInsertDic;
    }

    private static async Task<List<(int EmployeeId, string? NationalNumber, string? AccountNumber)>> DbConflicts(
        EmployeeDbContext dbContext, IEnumerable<Employee> entitiesToInsert, CancellationToken cancellationToken)
    {
        var nationalHashesToQuery = entitiesToInsert
            .Where(e => !string.IsNullOrEmpty(e.NationalNumberHash))
            .Select(e => e.NationalNumberHash).ToList();
        
        var accountHashesToQuery = entitiesToInsert
            .Where(e => !string.IsNullOrEmpty(e.AccountNumber))
            .Select(e => e.AccountNumberHash).ToList();

        var nationalConflicts = await dbContext.Employee
            .AsNoTracking()
            .Where(e => e.NationalNumberHash != null && nationalHashesToQuery.Contains(e.NationalNumberHash))
            .Select(e => new { e.EmployeeId, e.NationalNumber })
            .ToDictionaryAsync(e=> e.EmployeeId, cancellationToken);

        var accountConflicts = await dbContext.Employee
            .AsNoTracking()
            .Where(e => e.AccountNumberHash != null && accountHashesToQuery.Contains(e.AccountNumberHash))
            .Select(e => new { e.EmployeeId, e.AccountNumber })
            .ToDictionaryAsync(e=> e.EmployeeId, cancellationToken);

        var allEmployeeIds = nationalConflicts.Select(n => n.Key)
            .Union(accountConflicts.Keys); // Union on a HashSet/Keys automatically deduplicates
        
        var dbConflicts = allEmployeeIds.Select(id => (
            EmployeeId: id,
            NationalNumber: nationalConflicts.TryGetValue(id, out var natNum) ? natNum.NationalNumber : null,
            AccountNumber: accountConflicts.TryGetValue(id, out var accNum) ? accNum.AccountNumber : null
        )).ToList();

        return dbConflicts;
    }

    private static Employee? PrepareEmployee(BatchEmployee rawBatchEmployee, int currentUserId, IEmployeeTokenService tokenService)
    {
        // 1. Temporarily instantiate a shell or mock tokens if needed, 
        // but the cleanest way is to generate tokens based on the rawEmp name context
        // or pass an empty collection initially and update them downstream.
        // Let's use an empty list for creation, then generate and update properly.
        var emptyTokens = new List<EmployeeNameToken>();

        var employee = Employee.Create(rawBatchEmployee.Name, currentUserId, emptyTokens);

        if (employee == null)
        {
            return null; // Name validation failed inside Employee.Create
        }

        // 2. Map optional properties using domain business rules
        employee.UpdateNationalNumber(rawBatchEmployee.NationalNumber);
        employee.AccountNumber = rawBatchEmployee.AccountNumber; // Map directly if no custom domain method exists
        employee.Salary = rawBatchEmployee.Salary;
        employee.CreatedAt = DateTime.UtcNow;

        // 3. Generate and apply actual name tokens now that the name is cleaned inside the entity
        var tokens = tokenService.CreateTokens(employee, currentUserId);
        employee.UpdateNameTokens(tokens);

        return employee;
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