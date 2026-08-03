using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Benchmark.Database;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.EntityFrameworkCore;
using Benchmark.Implementations;
using System.Security.Cryptography;
using System.Text;

namespace Benchmark;

[MemoryDiagnoser]
public class PipelineBenchmark
{
    private SandboxDbContext _dbContext;
    private Guid _batchId;
    private List<IBatchProcessor> _processors;

    [GlobalSetup]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<SandboxDbContext>()
            .UseSqlServer("Server=DockerServer,1434;Database=RewardFlowDb_BenchmarkSandbox;User=sa;Password=Test123!@#;TrustServerCertificate=true")
            .Options;
        
        _dbContext = new SandboxDbContext(options);
        _dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        
        //_dbContext.Database.EnsureDeleted();
        _dbContext.Database.EnsureCreated();
        
        // Seed data
        Console.WriteLine("Start query get batch id");
        _batchId = _dbContext.BulkImportBatches.AsNoTracking().FirstOrDefault().Id;
        Console.WriteLine("got batch id");
        
        var batch = new BulkImportBatch { Id = _batchId, UserId = 1, RawPayloadJson = "{}" };
        _dbContext.BulkImportBatches.Add(batch);

        //GenerateAndImportTestData();

        _processors = new List<IBatchProcessor>
        {
            new Method1_Sequential(_dbContext),
            new Method2_ChannelsPipeline(_dbContext, 500),
            new Method2_ChannelsPipeline(_dbContext, 5000),
            new Method2_ChannelsPipeline(_dbContext, 50000),
            new Method3_BulkCopy(_dbContext)
        };
    }

    [Benchmark]
    public async Task RunAllMethods()
    {
        foreach (var processor in _processors)
        {
            await processor.ProcessBatchAsync(_batchId);
            // Clear tokens after each run to keep benchmark consistent
            await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM employee_name_tokens");
        }
    }

public void GenerateAndImportTestData()
{
        var random = new Random();

    // Helper to generate a dummy SHA256 hash string for security properties
    string ComputeHash(string input)
    {
        using (var sha256 = SHA256.Create())
        {
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes);
        }
    }

    // Helper to generate random numeric strings (like National IDs or Bank Accounts)
    string GenerateRandomNumericString(int length)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < length; i++)
        {
            sb.Append(random.Next(0, 10));
        }
        return sb.ToString();
    }

    // 2. Generate and add 10,000 Employees
    var employees = Enumerable.Range(1, 10000).Select(i =>
    {
        var fullName = NameGenerator.GenerateArabicName(1)[1];
        // Generate a pseudo-realistic 14-digit National Number (e.g., Egyptian/Arab region formats)
        var nationalNo = "2" + random.Next(70, 99).ToString() + random.Next(10, 13).ToString() + random.Next(10, 29).ToString() + GenerateRandomNumericString(7);
        var accountNo = "EG" + GenerateRandomNumericString(22); // Generates standard looking IBAN account

        return new Employee
        {
            Name = fullName,
            NationalNumber = nationalNo,
            AccountNumber = accountNo,
            NationalNumberHash = ComputeHash(nationalNo),
            AccountNumberHash = ComputeHash(accountNo),
            Salary = Math.Round((decimal)(random.NextDouble() * (25000 - 4000) + 4000), 2), // Random salary between 4,000 and 25,000
            FacultyId = random.Next(1, 10),     // Simulating 9 faculties
            DepartmentId = random.Next(1, 30),  // Simulating 29 departments
            CreatedBy = 1,                      // System user or Admin ID
            CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 365)), // Hired sometime within the last year
            JobTitle = (byte)random.Next(1, 6), // Assuming 5 distinct job titles represented as byte enums
            IsActive = true,
            Status = (byte)random.Next(1, 4)    // e.g., 1 = Probation, 2 = Permanent, 3 = Notice Period
        };
    }).ToList();

    _dbContext.Employees.AddRange(employees);
    _dbContext.SaveChanges(); // Saves employees and populates their database Identity IDs (`EmployeeId`)

    // 3. Generate BulkImportResults mapping directly to newly created EmployeeIds
    var results = employees.Select(emp => new BulkImportResult 
    { 
        BatchId = _batchId, 
        Tracker = Guid.NewGuid(), 
        IsSuccess = true, 
        EmployeeId = emp.EmployeeId, // Maps flawlessly to the exact generated DB identity ID
        Name = emp.Name              // Retains the exact generated Arabic name 
    }).ToList();

    _dbContext.BulkImportResults.AddRange(results);
    _dbContext.SaveChanges();
}


// Helper to generate Arabic names (3 to 5 parts)

}
