namespace Reward_Flow_v2.Rewards.SessionsReward;

internal interface IReward
{
    public Task<decimal> GetTotalAsync(int rewardId);
}