using System;
using System.Threading;
using System.Threading.Tasks;

namespace Benchmark.Implementations;

public interface IBatchProcessor
{
    string MethodName { get; }
    Task ProcessBatchAsync(Guid batchId);
}
