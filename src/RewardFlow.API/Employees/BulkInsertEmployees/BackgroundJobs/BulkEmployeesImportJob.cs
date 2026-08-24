using Microsoft.EntityFrameworkCore;
using Reward_Flow_v2.Employees.BulkInsertEmployees;
using Reward_Flow_v2.Employees.Data;
using Reward_Flow_v2.Employees.Data.Database;
using RewardFlow_API.Common.Interface;
using RewardFlow_API.Employees.BulkInsertEmployees.Interfaces;
using System.Text.Json;

namespace RewardFlow_API.Employees.BulkInsertEmployees.BackgroundJobs;

internal class BulkEmployeesImportJob(EmployeeDbContext dbContext, IUserContext userContext) : IBulkEmployeesImporter
{
    private BulkImportBatch batch;

    public async Task ExecuteAsync(Guid batchId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        userContext.SetTenantId(tenantId);

        var connection = dbContext.Database.GetDbConnection();

        var connectionString = connection.ConnectionString;

        // Open batch
        try
        {
            batch = await dbContext.BulkImportBatches.FirstOrDefaultAsync(b => b.Id == batchId);
            if (batch == null || batch.IsClosed) return;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        var rawEmployees = JsonSerializer.Deserialize<List<BatchEmployee>>(batch.RawPayloadJson);

        if (rawEmployees == null || rawEmployees.Count == 0)
        {
            batch.Completed();
            await dbContext.SaveChangesAsync(cancellationToken);
            return;
        }

        batch.Proccessing();
        await dbContext.SaveChangesAsync(cancellationToken);

        // Processing
        var results = new List<BulkImportResult>();

        var employeesToInsert = ProcessDistinctEmployees(rawEmployees, batch.Id, batch.UserId, results);

        await ResolveDatabaseConflictsAsync(employeesToInsert, results, cancellationToken);

        await InsertValidEmployeesAsync(employeesToInsert, results, cancellationToken);

        await SaveSuccessStats(results, batch, dbContext, cancellationToken);
    }

    private async Task InsertValidEmployeesAsync(
        Dictionary<Guid, Employee> entitiesToInsert, List<BulkImportResult> results,
        CancellationToken cancellationToken)
    {
        if (entitiesToInsert.Count == 0) return;

        await dbContext.Employee.AddRangeAsync(entitiesToInsert.Values, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        foreach ((Guid tracker, Employee employee) in entitiesToInsert)
        {
            results.Add(employee.Id > 0
                ? BulkImportResult.CreateSuccess(batch.Id, tracker, employee.Id, employee.Name)
                : BulkImportResult.CreateFailure(batch.Id, tracker, ErrorTypes.Unexpected,
                    "Unexpected error occurred."));
        }
    }

    private Dictionary<Guid, Employee> ProcessDistinctEmployees(
        List<BatchEmployee> rawEmployees, Guid batchId, int currentUserId, List<BulkImportResult> trackingResults)
    {
        var entitiesToInsertDic = new Dictionary<Guid, Employee>();
        var seenNationalHashes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var seenAccountHashes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var rawEntity in rawEmployees)
        {
            Guid trackerId = rawEntity.Tracker == Guid.Empty ? Guid.NewGuid() : rawEntity.Tracker;

            var entity = PrepareEmployee(rawEntity, currentUserId);

            // If creation failed, the name was empty or too short after regex cleaning
            if (entity is null || string.IsNullOrEmpty(entity.Name))
            {
                trackingResults.Add(BulkImportResult.CreateFailure(
                    batchId,
                    trackerId,
                    ErrorTypes.InvalidName,
                    "Name is invalid or too short after cleanup."
                ));
                continue;
            }

            // 1. Validation: Check for duplicate national numbers inside the payload
            if (!string.IsNullOrEmpty(entity.NationalNumber))
            {
                if (seenNationalHashes.Contains(entity.NationalNumber))
                {
                    trackingResults.Add(BulkImportResult.CreateFailure(
                        batchId,
                        trackerId,
                        ErrorTypes.DuplicateNationalNumber,
                        "Duplicate national number detected within the request."
                    ));
                    continue;
                }

                seenNationalHashes.Add(entity.NationalNumber);
            }

            // 2. Validation: Check for duplicate account numbers inside the payload
            if (!string.IsNullOrEmpty(entity.AccountNumber))
            {
                if (seenAccountHashes.Contains(entity.AccountNumber))
                {
                    trackingResults.Add(BulkImportResult.CreateFailure(
                        batchId,
                        trackerId,
                        ErrorTypes.DuplicateAccountNumber,
                        "Duplicate account number detected within the request."
                    ));
                    continue;
                }

                seenAccountHashes.Add(entity.AccountNumber);
            }

            entitiesToInsertDic.Add(trackerId, entity);
        }

        return entitiesToInsertDic;
    }

    private async Task ResolveDatabaseConflictsAsync(
        Dictionary<Guid, Employee> entitiesToInsert, List<BulkImportResult> results,
        CancellationToken cancellationToken)
    {
        var dbConflicts = await GetDbConflictsAsync(dbContext, entitiesToInsert.Values, cancellationToken);

        RemoveDbConflictedEntities(dbConflicts, entitiesToInsert, results);
    }

    private async Task SaveSuccessStats(List<BulkImportResult> results, BulkImportBatch batch,
        EmployeeDbContext dbContext, CancellationToken cancellationToken)
    {
        var successCount = results.Count(r => r.IsSuccess);
        batch.Completed(successCount);

        dbContext.AddRange(results);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<List<(int EmployeeId, string? NationalNumber, string? AccountNumber)>> GetDbConflictsAsync(
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
            .Select(e => new { EmployeeId = e.Id, e.NationalNumber })
            .ToDictionaryAsync(e => e.EmployeeId, cancellationToken);

        var accountConflicts = await dbContext.Employee
            .AsNoTracking()
            .Where(e => e.AccountNumberHash != null && accountHashesToQuery.Contains(e.AccountNumberHash))
            .Select(e => new { EmployeeId = e.Id, e.AccountNumber })
            .ToDictionaryAsync(e => e.EmployeeId, cancellationToken);

        var allEmployeeIds = nationalConflicts.Select(n => n.Key).Union(accountConflicts.Keys);

        return allEmployeeIds.Select(id => (
            EmployeeId: id,
            NationalNumber: nationalConflicts.TryGetValue(id, out var natNum) ? natNum.NationalNumber : null,
            AccountNumber: accountConflicts.TryGetValue(id, out var accNum) ? accNum.AccountNumber : null
        )).ToList();
    }

    private void RemoveDbConflictedEntities(
        List<(int EmployeeId, string? NationalNumber, string? AccountNumber)> dbConflicts,
        Dictionary<Guid, Employee> entitiesToInsert, List<BulkImportResult> results)
    {
        if (dbConflicts == null || dbConflicts.Count == 0) return;

        var employeesNationalMap = entitiesToInsert
            .Where(kvp => !string.IsNullOrEmpty(kvp.Value.NationalNumber))
            .ToDictionary(kvp => kvp.Value.NationalNumber!, kvp => kvp.Key, StringComparer.OrdinalIgnoreCase);

        var employeesAccountMap = entitiesToInsert
            .Where(kvp => !string.IsNullOrEmpty(kvp.Value.AccountNumber))
            .ToDictionary(kvp => kvp.Value.AccountNumber!, kvp => kvp.Key, StringComparer.OrdinalIgnoreCase);

        foreach (var conflict in dbConflicts)
        {
            bool isNationalConflict = employeesNationalMap.TryGetValue(conflict.NationalNumber!, out var trackerId);
            bool isAccountConflict = !isNationalConflict &&
                                     employeesAccountMap.TryGetValue(conflict.AccountNumber!, out trackerId);

            if (!isNationalConflict && !isAccountConflict) continue;

            entitiesToInsert.Remove(trackerId);

            string message = isNationalConflict
                ? "An employee with the same national number already exists in the database."
                : "An employee with the same account number already exists in the database.";

            results.Add(BulkImportResult.CreateFailure(batch.Id, trackerId, ErrorTypes.DatabaseConflict,
                message));
        }
    }

    private Employee? PrepareEmployee(BatchEmployee rawBatchEmployee, int currentUserId)
    {
        // Pass an empty token collection. The tokens job will update this collection later.
        var emptyTokens = new List<EmployeeNameToken>();
        var employee = Employee.Create(rawBatchEmployee.Name, currentUserId, emptyTokens);

        if (employee == null) return null;

        employee.UpdateNationalNumber(rawBatchEmployee.NationalNumber);
        employee.AccountNumber = rawBatchEmployee.AccountNumber;
        employee.Salary = rawBatchEmployee.Salary;
        employee.CreatedAt = DateTime.UtcNow;
        employee.TenantId = batch.TenantId;

        return employee;
    }
}