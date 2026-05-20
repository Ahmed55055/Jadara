using Reward_Flow_v2.Rewards.SessionsReward.Interface;

namespace Reward_Flow_v2.Rewards.SessionsReward.Common;

public class SessionsRewardCalculator : ISessionRewardCalculator
{
    public int CalculateSessions(int numberOfStudents)
    {
        if (numberOfStudents < 5)
            return (int)Math.Ceiling(numberOfStudents / 5.0);
        
        return (int)Math.Round(numberOfStudents / 5.0, MidpointRounding.AwayFromZero);
    }

    public decimal CalculateTotal(int numOfSessions, decimal salary, decimal percentage)
    {
        return numOfSessions * salary * percentage;
    }
}