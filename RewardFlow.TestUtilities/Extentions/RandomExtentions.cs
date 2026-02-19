namespace RewardFlow.TestUtilities.Extentions;

public static class RandomExtentions
{
    public static DateTime NextDateTime(this Random random, DateTime start, DateTime end)
    {
        if (start >= end) 
            throw new ArgumentException("Start date must be before end date.");

        // Calculate the range in ticks
        long range = end.Ticks - start.Ticks;
        
        // Generate a random number of ticks within that range
        long randomTicks = (long)(random.NextDouble() * range);

        return new DateTime(start.Ticks + randomTicks);
    }
    
    public static DateTime NextDate(this Random random, DateTime start, DateTime end)
    {
        int range = (end - start).Days;
        return start.AddDays(random.Next(range));
    }
}