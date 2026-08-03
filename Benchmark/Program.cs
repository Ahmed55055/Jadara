using Benchmark;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks; // Explicitly included for the auto-generator
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.Emit; // Bypasses external build pipeline
using Reward_Flow_v2.Common.Tokenization;
using Reward_Flow_v2.Employees.Common;
using Reward_Flow_v2.Employees.Data;
using RewardFlow_API.User.AuthService;
using System.Collections.Concurrent;
using System.Text.Json;

// Run the benchmark
/*
Console.WriteLine("--- Running Baselines ---");
BenchmarkRunner.Run<BaselineBenchmarks>();
*/

Console.WriteLine("--- Running Pipeline Parameter Matrix ---");
BenchmarkRunner.Run<PipelineTuningBenchmarksJson>();


// =========================================================================
// CUSTOM BENCHMARKCONFIG: Injects the custom Median Live Memory Column
// =========================================================================
public class PipelineTuningConfig : ManualConfig
{
    public PipelineTuningConfig()
    {
        WithOptions(ConfigOptions.DisableOptimizationsValidator);
        AddJob(Job.MediumRun.WithToolchain(InProcessEmitToolchain.Instance));

        // Register our custom column provider
        AddColumn(new MedianLiveMemoryColumn());
    }
}
// =========================================================================
// THE BENCHMARK CLASS
// =========================================================================

[Config(typeof(PipelineTuningConfig))]
[MemoryDiagnoser]
public class PipelineTuningBenchmarksJson
{
    // Holds the live tracking samples for our custom metric column
    public static readonly ConcurrentDictionary<string, List<double>> MemorySamples = new();

    // The data list that will scale up and down dynamically depending on the current parameter variation
    private List<string> _activeNamesList = [];

    // --- 4D TUNING MATRIX PARAMETERS ---

    [Params(10000)] public int NameCount { get; set; }

    [Params(25_000)] public int ChannelCapacity { get; set; }

    [Params(50_000)] public int DbBatchSize { get; set; }

    [Params(10)] public int BaseNetworkLatencyMs { get; set; }

    // This executes automatically before every individual parameter variation run
    [GlobalSetup]
    public void Setup()
    {
        // Dynamically scale the dataset size to exactly what BenchmarkDotNet demands for this specific iteration
        _activeNamesList = NameGenerator.GenerateArabicName(NameCount);
    }

    [Benchmark]
    public async Task TokenGenerationParallelPipeline()
    {
        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        string lookupKey = $"{NameCount}_{ChannelCapacity}_{DbBatchSize}_{BaseNetworkLatencyMs}";
        var localSamples = MemorySamples.GetOrAdd(lookupKey, _ => new List<double>());

        var tokenChannel = Channel.CreateBounded<EmployeeNameToken>(new BoundedChannelOptions(ChannelCapacity)
        {
            FullMode = BoundedChannelFullMode.Wait, SingleReader = true, SingleWriter = false
        });

        // SingleReader = false lets multiple concurrent workers drain this channel simultaneously
        var batchChannel = Channel.CreateBounded<List<EmployeeNameToken>>(new BoundedChannelOptions(10)
        {
            FullMode = BoundedChannelFullMode.Wait, SingleReader = false, SingleWriter = true
        });

        var isRunning = true;
        var memoryMonitorTask = Task.Run(async () =>
        {
            while (isRunning)
            {
                long currentMemoryBytes = GC.GetTotalMemory(false);
                lock (localSamples)
                {
                    localSamples.Add(currentMemoryBytes / 1024.0 / 1024.0);
                }

                await Task.Delay(2);
            }
        });

        // --- STAGE 1: NON-BLOCKING PARALLEL ASYNC PRODUCER ---
        var producerTask = Task.Run(async () =>
        {
            var tokenizer = new EmployeeTokenService(null, new Tokenizer());

            // Throttling maintained: restricts concurrent execution to Core Count - 1
            int maxCoresToUse = Math.Max(1, Environment.ProcessorCount - 1);
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = maxCoresToUse, CancellationToken = cancellationToken
            };

            try
            {
                // Migrated to ForEachAsync to replace synchronous .GetAwaiter().GetResult() blocking
                await Parallel.ForEachAsync(_activeNamesList, parallelOptions, async (name, token) =>
                {
                    var generatedTokens = tokenizer.CreateTokens(name, 1, 1);
                    foreach (var tokenItem in generatedTokens)
                    {
                        // Clean, non-blocking asynchronous streaming
                        await tokenChannel.Writer.WriteAsync(tokenItem, token);
                    }
                });
            }
            finally
            {
                tokenChannel.Writer.Complete();
            }
        }, cancellationToken);

        // --- STAGE 2: FAST CONSUMER & BATCHER ---
        var batcherTask = Task.Run(async () =>
        {
            try
            {
                var batch = new List<EmployeeNameToken>(DbBatchSize);

                await foreach (var token in tokenChannel.Reader.ReadAllAsync(cancellationToken))
                {
                    batch.Add(token);
                    if (batch.Count >= DbBatchSize)
                    {
                        await batchChannel.Writer.WriteAsync(batch, cancellationToken);
                        batch = new List<EmployeeNameToken>(DbBatchSize);
                    }
                }

                if (batch.Count > 0)
                {
                    await batchChannel.Writer.WriteAsync(batch, cancellationToken);
                }
            }
            finally
            {
                batchChannel.Writer.Complete();
            }
        }, cancellationToken);

        // --- STAGE 3: MULTI-THREADED CONCURRENT SENDER POOL ---
        // Spawns 3 parallel background tasks to process serialization during database idle periods
        int totalWorkers = 3;
        var senderTasks = new Task[totalWorkers];

        for (int i = 0; i < totalWorkers; i++)
        {
            senderTasks[i] = Task.Run(async () =>
            {
                await foreach (var readyBatch in batchChannel.Reader.ReadAllAsync(cancellationToken))
                {
                    string jsonPayload = JsonSerializer.Serialize(readyBatch);

                    double transmissionOverhead = readyBatch.Count * 0.0004;
                    int totalCalculatedLatency = BaseNetworkLatencyMs + (int)Math.Round(transmissionOverhead);

                    await Task.Delay(totalCalculatedLatency);
                }
            }, cancellationToken);
        }

        await producerTask;
        await batcherTask;
        await Task.WhenAll(senderTasks);

        isRunning = false;
        await memoryMonitorTask;
    }

    [Benchmark]
    public async Task TokenGenerationParallelPipelineJsonOptimized()
    {
        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        string lookupKey = $"{NameCount}_{ChannelCapacity}_{DbBatchSize}_{BaseNetworkLatencyMs}";
        var localSamples = MemorySamples.GetOrAdd(lookupKey, _ => new List<double>());

        var tokenChannel = Channel.CreateBounded<EmployeeNameToken>(new BoundedChannelOptions(ChannelCapacity)
        {
            FullMode = BoundedChannelFullMode.Wait, SingleReader = true, SingleWriter = false
        });

        // The channel now holds the active background serialization Tasks.
        // Capacity is set to 5 so we keep up to 5 massive JSON blocks warm and ready in the pipeline.
        var serializedJsonChannel = Channel.CreateBounded<Task<string>>(new BoundedChannelOptions(5)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true, // Strict single reader to preserve database insertion order across batches
            SingleWriter = true
        });

        var isRunning = true;
        var memoryMonitorTask = Task.Run(async () =>
        {
            while (isRunning)
            {
                long currentMemoryBytes = GC.GetTotalMemory(false);
                lock (localSamples)
                {
                    localSamples.Add(currentMemoryBytes / 1024.0 / 1024.0);
                }

                await Task.Delay(2);
            }
        });

        // --- STAGE 1: PARALLEL PRODUCER ---
        var producerTask = Task.Run(async () =>
        {
            var tokenizer = new EmployeeTokenService(null, new Tokenizer());
            int maxCoresToUse = Math.Max(1, Environment.ProcessorCount - 1);
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = maxCoresToUse, CancellationToken = cancellationToken
            };

            try
            {
                await Parallel.ForEachAsync(_activeNamesList, parallelOptions, async (name, token) =>
                {
                    var generatedTokens = tokenizer.CreateTokens(name, 1, 1);
                    foreach (var tokenItem in generatedTokens)
                    {
                        await tokenChannel.Writer.WriteAsync(tokenItem, token);
                    }
                });
            }
            finally
            {
                tokenChannel.Writer.Complete();
            }
        }, cancellationToken);

        // --- STAGE 2: IMMEDIATE PROACTIVE SERIALIZATION BATCHER ---
        var batcherTask = Task.Run(async () =>
        {
            try
            {
                var batch = new List<EmployeeNameToken>(DbBatchSize);

                await foreach (var token in tokenChannel.Reader.ReadAllAsync(cancellationToken))
                {
                    batch.Add(token);
                    if (batch.Count >= DbBatchSize)
                    {
                        // Local copy to prevent race conditions during list reallocation
                        var batchToSerialize = batch;

                        // Fire-and-forget the serialization onto the thread pool IMMEDIATELY
                        Task<string> serializationTask = Task.Run(() => JsonSerializer.Serialize(batchToSerialize),
                            cancellationToken);

                        // Push the active task handle down the line without awaiting it here
                        await serializedJsonChannel.Writer.WriteAsync(serializationTask, cancellationToken);

                        batch = new List<EmployeeNameToken>(DbBatchSize);
                    }
                }

                if (batch.Count > 0)
                {
                    var finalBatch = batch;
                    Task<string> serializationTask =
                        Task.Run(() => JsonSerializer.Serialize(finalBatch), cancellationToken);
                    await serializedJsonChannel.Writer.WriteAsync(serializationTask, cancellationToken);
                }
            }
            finally
            {
                serializedJsonChannel.Writer.Complete();
            }
        }, cancellationToken);

        // --- STAGE 3: ZERO-WAIT DATABASE CONTROLLER ---
        var senderTask = Task.Run(async () =>
        {
            await foreach (var serializationTask in serializedJsonChannel.Reader.ReadAllAsync(cancellationToken))
            {
                // By the time the database roundtrip delay finishes, the next task in the queue 
                // has already completed its serialization in the background.
                string jsonPayload = await serializationTask;

                // Calculate latency simulated on payload scale
                // (Using an arbitrary item multiplier here based on your target batch sizing)
                int itemsCount = DbBatchSize; // approximate or track exact counts if preferred
                double transmissionOverhead = itemsCount * 0.0004;
                int totalCalculatedLatency = BaseNetworkLatencyMs + (int)Math.Round(transmissionOverhead);

                // Execute database roundtrip write simulation
                await Task.Delay(totalCalculatedLatency);
            }
        }, cancellationToken);

        await producerTask;
        await batcherTask;
        await senderTask;

        isRunning = false;
        await memoryMonitorTask;
    }
}

// =========================================================================
// COLUMN PARSING LAYOUT
// =========================================================================
public class MedianLiveMemoryColumn : IColumn
{
    public string Id => nameof(MedianLiveMemoryColumn);
    public string ColumnName => "Median Live Heap";
    public bool IsAvailable(Summary summary) => true;
    public bool AlwaysShow => true;
    public ColumnCategory Category => ColumnCategory.Metric;
    public int PriorityInCategory => 1;
    public bool IsNumeric => true;
    public UnitType UnitType => UnitType.Size;
    public string Legend => "The median active retained memory layout on the managed heap during method processing.";

    public string GetValue(Summary summary, BenchmarkCase benchmarkCase)
    {
        var count = benchmarkCase.Parameters["NameCount"];
        var cap = benchmarkCase.Parameters["ChannelCapacity"];
        var batch = benchmarkCase.Parameters["DbBatchSize"];
        var latency = benchmarkCase.Parameters["BaseNetworkLatencyMs"];
        string lookupKey = $"{count}_{cap}_{batch}_{latency}";

        if (PipelineTuningBenchmarks.MemorySamples.TryGetValue(lookupKey, out var samples) && samples.Count > 0)
        {
            double median;
            lock (samples)
            {
                var sortedSamples = samples.OrderBy(s => s).ToList();
                int sampleCount = sortedSamples.Count;
                if (sampleCount % 2 == 0)
                {
                    median = (sortedSamples[sampleCount / 2 - 1] + sortedSamples[sampleCount / 2]) / 2.0;
                }
                else
                {
                    median = sortedSamples[sampleCount / 2];
                }
            }

            return $"{median:F2} MB";
        }

        return "N/A";
    }

    public string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style) =>
        GetValue(summary, benchmarkCase);

    public bool IsDefault(Summary summary, BenchmarkCase benchmarkCase) => false;
}

[Config(typeof(PipelineTuningConfig))]
[MemoryDiagnoser]
public class PipelineTuningBenchmarks
{
    // Holds the live tracking samples for our custom metric column
    public static readonly ConcurrentDictionary<string, List<double>> MemorySamples = new();

    // The data list that will scale up and down dynamically depending on the current parameter variation
    private List<string> _activeNamesList = [];

    // --- 4D TUNING MATRIX PARAMETERS ---

    [Params(200, 1000, 5000, 10000)] public int NameCount { get; set; }

    [Params(25_000, 50_000)] public int ChannelCapacity { get; set; }

    [Params(10_000, 25_000, 50_000)] public int DbBatchSize { get; set; }

    [Params(10)] public int NetworkLatencyMs { get; set; }

    // This executes automatically before every individual parameter variation run
    [GlobalSetup]
    public void Setup()
    {
        // Dynamically scale the dataset size to exactly what BenchmarkDotNet demands for this specific iteration
        _activeNamesList = NameGenerator.GenerateArabicName(NameCount);
    }

    [Benchmark]
    public async Task TokenGenerationParallelPipeline()
    {
        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        // Identify this specific execution branch variant uniquely
        string lookupKey = $"{ChannelCapacity}_{DbBatchSize}_{NetworkLatencyMs}";
        var localSamples = MemorySamples.GetOrAdd(lookupKey, _ => new List<double>());

        var channel = Channel.CreateBounded<EmployeeNameToken>(new BoundedChannelOptions(ChannelCapacity)
        {
            FullMode = BoundedChannelFullMode.Wait, SingleReader = true, SingleWriter = false
        });

        // --- BACKGROUND MONITOR ---
        var isRunning = true;
        var memoryMonitorTask = Task.Run(async () =>
        {
            while (isRunning)
            {
                long currentMemoryBytes = GC.GetTotalMemory(false);
                lock (localSamples)
                {
                    localSamples.Add(currentMemoryBytes / 1024.0 / 1024.0); // Store sample in MB
                }

                await Task.Delay(2); // Tight sampling window for accuracy
            }
        });

        // --- PRODUCER ---
        var producerTask = Task.Run(() =>
        {
            var tokenizer = new EmployeeTokenService(null, new Tokenizer());
            int maxCoresToUse = Math.Max(1, Environment.ProcessorCount - 1);
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = maxCoresToUse, CancellationToken = cancellationToken
            };

            try
            {
                Parallel.ForEach(_activeNamesList, parallelOptions, name =>
                {
                    var generatedTokens = tokenizer.CreateTokens(name, 1, 1);
                    foreach (var token in generatedTokens)
                    {
                        while (!channel.Writer.TryWrite(token))
                        {
                            channel.Writer.WaitToWriteAsync(cancellationToken).AsTask().GetAwaiter().GetResult();
                        }
                    }
                });
            }
            finally
            {
                channel.Writer.Complete();
            }
        }, cancellationToken);

        // --- CONSUMER ---
        var consumerTask = Task.Run(async () =>
        {
            var batch = new List<EmployeeNameToken>(DbBatchSize);

            await foreach (var token in channel.Reader.ReadAllAsync(cancellationToken))
            {
                batch.Add(token);
                if (batch.Count >= DbBatchSize)
                {
                    await Task.Delay(NetworkLatencyMs);
                    batch.Clear();
                }
            }

            if (batch.Count > 0)
            {
                await Task.Delay(NetworkLatencyMs);
            }
        }, cancellationToken);

        await Task.WhenAll(producerTask, consumerTask);

        isRunning = false;
        await memoryMonitorTask;
    }
}

/*
// =========================================================================
// BENCHMARKDOTNET COLUMN ENGINE: Calculates the Median and appends it to report
// =========================================================================
public class MedianLiveMemoryColumn : IColumn
{
    public string Id => nameof(MedianLiveMemoryColumn);
    public string ColumnName => "Median Live Heap";
    public bool IsAvailable(Summary summary) => true;
    public bool AlwaysShow => true;
    public ColumnCategory Category => ColumnCategory.Metric;
    public int PriorityInCategory => 1;
    public bool IsNumeric => true;
    public UnitType UnitType => UnitType.Size;
    public string Legend => "The median active retained memory layout on the managed heap during method processing.";

    public string GetValue(Summary summary, BenchmarkCase benchmarkCase)
    {
        // Extract the target parameters BenchmarkDotNet is evaluating right now
        var cap = benchmarkCase.Parameters["ChannelCapacity"];
        var batch = benchmarkCase.Parameters["DbBatchSize"];
        var latency = benchmarkCase.Parameters["NetworkLatencyMs"];
        string lookupKey = $"{cap}_{batch}_{latency}";

        if (PipelineTuningBenchmarks.MemorySamples.TryGetValue(lookupKey, out var samples) && samples.Count > 0)
        {
            double median;
            lock (samples)
            {
                var sortedSamples = samples.OrderBy(s => s).ToList();
                int count = sortedSamples.Count;
                if (count % 2 == 0)
                {
                    if (count % 2 == 0)
                    {
                        median = (sortedSamples[count / 2 - 1] + sortedSamples[count / 2]) / 2.0;
                    }
                    else
                    {
                        median = sortedSamples[count / 2];
                    }
                }
                else
                {
                    median = sortedSamples[count / 2];
                }
            }
            return $"{median:F2} MB";
        }

        return "N/A";
    }

    public string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style) => GetValue(summary, benchmarkCase);
    public bool IsDefault(Summary summary, BenchmarkCase benchmarkCase) => false;
}*/

// ==========================================
// SHARED TOOLCHAIN CONFIG
// ==========================================
public class InProcessConfig : ManualConfig
{
    public InProcessConfig()
    {
        WithOptions(ConfigOptions.DisableOptimizationsValidator);
        AddJob(Job.MediumRun.WithToolchain(InProcessEmitToolchain.Instance));
    }
}


[Config(typeof(FastDebugConfig))]
[MemoryDiagnoser]
public class BaselineBenchmarks // Marked explicitly public
{
    private class FastDebugConfig : ManualConfig
    {
        public FastDebugConfig()
        {
            WithOptions(ConfigOptions.DisableOptimizationsValidator);

            // Using InProcessEmitToolchain ensures that Task-returning methods 
            // are compiled directly inside this project instead of a broken isolated file.
            AddJob(Job.MediumRun.WithToolchain(InProcessEmitToolchain.Instance));
        }
    }

    private static readonly List<string> GeneratedNames = NameGenerator.GenerateArabicName(10_000);

    [ParamsSource(nameof(GetTestData))] public List<string> Names { get; set; }

    public IEnumerable<List<string>> GetTestData() => [GeneratedNames];

    [Benchmark(Baseline = true)]
    public List<EmployeeNameToken> Sequential()
    {
        List<EmployeeNameToken> tokens = [];
        var tokenizer = new EmployeeTokenService(null, new Tokenizer());
        foreach (var name in Names)
        {
            tokens.AddRange(tokenizer.CreateTokens(name, 1, 1));
        }

        return tokens;
    }

    [Benchmark]
    public List<EmployeeNameToken> ParallelPLINQ()
    {
        var tokenizer = new EmployeeTokenService(null, new Tokenizer());
        return Names
            .AsParallel()
            .SelectMany(name => tokenizer.CreateTokens(name, 1, 1))
            .ToList();
    }
}