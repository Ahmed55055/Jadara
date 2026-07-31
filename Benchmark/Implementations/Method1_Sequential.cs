using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Benchmark.Database;
using Reward_Flow_v2.Common.Tokenization;

namespace Benchmark.Implementations;

public static class TokenGenerationHelper
{
    private static readonly Tokenizer Tokenizer = new();

    public static List<EmployeeNameToken> CreateTokens(string name, int employeeId, int userId)
    {
        var tokens = new List<EmployeeNameToken>();

        var twoGrams = Tokenizer.TokenizeToNGrams(name, 2, false);
        tokens.AddRange(twoGrams.Select(token => new EmployeeNameToken
        {
            UserId = userId,
            TokenHashed = Tokenizer.HashToken(token),
            N = 2,
            EmployeeId = employeeId
        }));

        var threeGrams = Tokenizer.TokenizeToNGrams(name, 3, true);
        tokens.AddRange(threeGrams.Select(token => new EmployeeNameToken
        {
            UserId = userId,
            TokenHashed = Tokenizer.HashToken(token),
            N = 3,
            EmployeeId = employeeId
        }));

        return tokens;
    }
}

public class Method1_Sequential : IBatchProcessor
{
    private readonly SandboxDbContext _dbContext;

    public Method1_Sequential(SandboxDbContext dbContext)
    {
        _dbContext = dbContext;
        _dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    public string MethodName => "Sequential (EF Core AddRange)";

    public async Task ProcessBatchAsync(Guid batchId)
    {
        Console.WriteLine("making first query");
        
        var importedEmployees = _dbContext.BulkImportResults
            .AsNoTracking()
            .Where(r => r.BatchId == batchId && r.IsSuccess && r.EmployeeId.HasValue)
            .Select(r => new { r.EmployeeId, r.Name })
            .ToList();

        Console.WriteLine("First query is done");
        var nameTokens = new ConcurrentBag<EmployeeNameToken>();

        Parallel.ForEach(importedEmployees, new ParallelOptions
        {
            MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount - 1),
        }, emp =>
        {
            var tokens = TokenGenerationHelper.CreateTokens(emp.Name, emp.EmployeeId!.Value, 1);
            foreach (var token in tokens)
            {
                nameTokens.Add(token);
            }
        });
        Console.WriteLine("Method 1: Tokens genrated");

        _dbContext.EmployeeNameTokens.AddRange(nameTokens.Take(500));
        Console.WriteLine("Aded to the dbcontext");
        
        await _dbContext.SaveChangesAsync();
        Console.WriteLine("Method 1: saved tokens");
    }
}
