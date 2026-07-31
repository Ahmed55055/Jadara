namespace RewardFlow.TestUtilities.UtilityClasses;

public record WaiterResult(bool IsDone, bool IsProcessing);

public class Waiter
{
    public static async Task Wait(Func<Task<WaiterResult>> task, int timeoutSec, int stableChecksBreakCount, string? stableErrorMessage = null, int delayMs = 1000)
    {
        var timeout = TimeSpan.FromSeconds(timeoutSec);
        var start = DateTime.UtcNow;
        
        var stableChecks = 0;
        
        while (DateTime.UtcNow - start < timeout)
        {
            var result = await task();
            
            if(result.IsDone)
                return;

            if (result.IsProcessing)
                stableChecks = 0;
            else
            {
                stableChecks++;
                
                if (stableChecks >= stableChecksBreakCount)
                    throw new Exception(stableErrorMessage?? "Stable check limit reached. Background job likely failed or stalled.");
            }

            await Task.Delay(delayMs); // Poll every 1 second
        }
        
        throw new TimeoutException("Waiter timed out!");
    }

}