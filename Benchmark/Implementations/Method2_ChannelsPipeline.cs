using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Benchmark.Database;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;

namespace Benchmark.Implementations;

public class Method2_ChannelsPipeline : IBatchProcessor
{
    private readonly SandboxDbContext _dbContext;
    private readonly int _batchSize;
    private const int MaxDegreeOfParallelism = 4;

    public Method2_ChannelsPipeline(SandboxDbContext dbContext, int batchSize)
    {
        _dbContext = dbContext;
        _batchSize = batchSize;
    }

    public string MethodName => $"Channels Pipeline (BatchSize: {_batchSize})";

    public async Task ProcessBatchAsync(Guid batchId)
    {
        var batch = await _dbContext.BulkImportBatches
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == batchId);
        if (batch == null) return;

        var userId = batch.UserId;

        var importedEmployees = await _dbContext.BulkImportResults
            .AsNoTracking()
            .Where(r => r.BatchId == batchId && r.IsSuccess && r.EmployeeId.HasValue)
            .Select(r => new { r.EmployeeId, r.Name })
            .ToListAsync();

        var employeeChannel = Channel.CreateBounded< (int EmployeeId, string Name)>(new BoundedChannelOptions(importedEmployees.Count) { SingleWriter = true, SingleReader = false });
        var tokenChannel = Channel.CreateBounded<EmployeeNameToken>(new BoundedChannelOptions(importedEmployees.Count * 2) { SingleWriter = false, SingleReader = true });

        var producerTask = Task.Run(async () =>
        {
            foreach (var emp in importedEmployees)
            {
                await employeeChannel.Writer.WriteAsync((emp.EmployeeId!.Value, emp.Name) );
            }
            employeeChannel.Writer.Complete();
        });

        var tokenizerTasks = Enumerable.Range(0, MaxDegreeOfParallelism).Select(i => Task.Run(async () =>
        {
            await foreach (var emp in employeeChannel.Reader.ReadAllAsync())
            {
                var tokens = TokenGenerationHelper.CreateTokens(emp.Name, emp.EmployeeId, userId);
                foreach (var token in tokens)
                {
                    await tokenChannel.Writer.WriteAsync(token);
                }
            }
        })).ToArray();

        var consumerTask = Task.Run(async () =>
        {
            var buffer = new List<EmployeeNameToken>(_batchSize);
            await foreach (var token in tokenChannel.Reader.ReadAllAsync())
            {
                buffer.Add(token);
                if (buffer.Count >= _batchSize)
                {
                    await _dbContext.BulkInsertAsync(buffer );
                    buffer.Clear();
                }
            }
            if (buffer.Count > 0)
            {
                await _dbContext.BulkInsertAsync(buffer);
            }
        });

        await producerTask;
        await Task.WhenAll(tokenizerTasks);
        tokenChannel.Writer.Complete();
        await consumerTask;
    }
}
