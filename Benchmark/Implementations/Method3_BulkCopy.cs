using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Benchmark.Database;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;

namespace Benchmark.Implementations;

public class Method3_BulkCopy : IBatchProcessor
{
    private readonly SandboxDbContext _dbContext;

    public Method3_BulkCopy(SandboxDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public string MethodName => "Bulk Copy (EF Core BulkExtensions)";

    public async Task ProcessBatchAsync(Guid batchId)
    {
        var batch = await _dbContext.BulkImportBatches
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == batchId );
        if (batch == null) return;

        var userId = batch.UserId;

        var importedEmployees = await _dbContext.BulkImportResults
            .AsNoTracking()
            .Where(r => r.BatchId == batchId && r.IsSuccess && r.EmployeeId.HasValue)
            .Select(r => new { r.EmployeeId, r.Name })
            .ToListAsync();

        var nameTokens = new ConcurrentBag<EmployeeNameToken>();

        Parallel.ForEach(importedEmployees, new ParallelOptions
        {
            MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount - 1),
        }, emp =>
        {
            var tokens = TokenGenerationHelper.CreateTokens(emp.Name, emp.EmployeeId!.Value, userId);
            foreach (var token in tokens)
            {
                nameTokens.Add(token);
            }
        });

        await _dbContext.BulkInsertAsync(nameTokens.ToList());
    }
}
