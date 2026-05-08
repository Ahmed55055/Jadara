namespace Reward_Flow_v2.Rewards;

public interface IReward
{
    Task Calculate();
    Task<float> GetTotal();
    Task<bool> IsComplete();
    Task<bool> IsClosed();
    Task<bool> Delete();
    
}