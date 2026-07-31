using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Reward_Flow_v2.Employees.Data;
using Reward_Flow_v2.Employees.Data.Database;
using RewardFlow_API.Common.Interface;
using RewardFlow_API.Employees.Common;
using System.Data.Common;
using System.Threading.Channels;

namespace Reward_Flow_v2.Employees.BulkInsertEmployees;

internal class TokenBackgroundJob(
    EmployeeDbContext dbContext,
    IEmployeeTokenService tokenService,
    IUserContext userContext,
    IBulkInserter<EmployeeNameToken> bulkInserter,
    ILogger<TokenBackgroundJob> logger) : ITokenBackgroundJob
{
    private record EmployeeImportDto(int EmployeeId, string Name);

    private BulkImportBatch _batch;
    public async Task GenerateBatchTokens(Guid batchId, Guid tenantId)
    {
        userContext.SetTenantId(tenantId);
        var connString = dbContext.Database.GetConnectionString();
        
        var connection = dbContext.Database.GetDbConnection();

        logger.LogInformation(
            "Provider: {Provider}, ConnectionString: {ConnectionString}",
            connection.GetType().FullName,
            connection.ConnectionString);

        List<EmployeeImportDto> employees;
        
        try
        {
            _batch = await dbContext.BulkImportBatches.FirstOrDefaultAsync(b => b.Id == batchId);
            
            if(_batch is null) return;
            
            employees = await GetImportedEmployeesIds(batchId);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occurred while fetching employees for token generation");
            throw;
        }
        
        const int batchSize = 50_000;

        var tokensChannel = Channel.CreateBounded<EmployeeNameToken>(new BoundedChannelOptions(2500)
        {
            FullMode = BoundedChannelFullMode.Wait, SingleReader = true, SingleWriter = false
        });

        var batchChannel = Channel.CreateBounded<List<EmployeeNameToken>>(new BoundedChannelOptions(batchSize));

        // --- STAGE 1: PRODUCER ---
        var producerTask = Task.Run(() => ProduceTokens(employees, _batch.UserId, tokensChannel));

        // --- STAGE 2: BATCHER ---
        var batcherTask = Task.Run(() => BatchTokens(batchSize, tokensChannel, batchChannel));

        // --- STAGE 3: DATABASE SENDER ---
        var dbSenderTask = Task.Run(() => SendToDatabase(batchChannel));
        
        try
        {
            await producerTask;
            await batcherTask;
            await dbSenderTask;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occurred during token generation for batch {BatchId}", batchId);
            throw;
        }
    }

    private async Task ProduceTokens(List<EmployeeImportDto> employees, int userId,
        Channel<EmployeeNameToken> tokensChannel)
    {
        int maxCoresToUse = Math.Max(1, Environment.ProcessorCount - 1);
        var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = maxCoresToUse };

        try
        {
            await Parallel.ForEachAsync(employees, parallelOptions, async (emp, _) =>
            {
                var generatedTokens = tokenService.CreateTokens(emp.Name, emp.EmployeeId, userId);
                foreach (var token in generatedTokens)
                    await tokensChannel.Writer.WriteAsync(token);
            });
        }
        finally
        {
            tokensChannel.Writer.Complete();
        }
    }

    private async Task BatchTokens(int batchSize, Channel<EmployeeNameToken> tokensChannel,
        Channel<List<EmployeeNameToken>> batchChannel)
    {
        try
        {
            var batchSizeSafeMargin = (int)(batchSize * 1.1);
            var batch = new List<EmployeeNameToken>(batchSizeSafeMargin);

            await foreach (var token in tokensChannel.Reader.ReadAllAsync())
            {
                batch.Add(token);
                
                if (batch.Count <= batchSize)
                    continue;

                await batchChannel.Writer.WriteAsync(batch);
                batch = new List<EmployeeNameToken>(batchSizeSafeMargin);
            }

            if (batch.Count > 0)
                batchChannel.Writer.WriteAsync(batch);
        }
        finally
        {
            batchChannel.Writer.Complete();
        }
    }

    private async Task SendToDatabase(Channel<List<EmployeeNameToken>> batchChannel)
    {
        await foreach (var readyBatch in batchChannel.Reader.ReadAllAsync())
        {
            try
            {
                await ExecuteWithRetry(() => bulkInserter.BulkInsertAsync(readyBatch, _batch.UserId, _batch.TenantId));
                logger.LogInformation("Batch of {Count} tokens saved successfully.", readyBatch.Count);
            }
            catch (Exception e)
            {
                logger.LogCritical(e, "CRITICAL: Batch failed permanently after retries.");
            }
        }
    }

    private async Task ExecuteWithRetry(Func<Task> function, int retries = 3)
    {
        int
            delay = 100; // 100ms: because waiting too much will affect the overall preformance very slow. and this is justifible time to the execution and round trip

        for (int attempt = 1; attempt <= retries; attempt++)
        {
            try
            {
                await function();
                return;
            }
            catch (Exception e) when (attempt < retries)
            {
                logger.LogWarning(e, "Transient error on attempt {Attempt} of {MaxRetries}. Retrying in {Delay}ms...",
                    attempt, retries, delay);
                await Task.Delay(delay);
                delay *= 2;
            }
        }
    }

    private async Task<List<EmployeeImportDto>> GetImportedEmployeesIds(Guid batchId)
    {
        return await dbContext.BulkImportResults
            .AsNoTracking()
            .Where(r => r.BatchId == batchId && r.EmployeeId != null)
            .Select(r => new EmployeeImportDto(r.EmployeeId!.Value, r.Name))
            .ToListAsync();
    }
}