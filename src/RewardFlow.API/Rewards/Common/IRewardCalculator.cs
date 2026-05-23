namespace Reward_Flow_v2.Rewards.Common;

public interface IRewardCalculator
{
    decimal CalculateTotal(int numOfSessions, decimal salary, decimal percentage);
}