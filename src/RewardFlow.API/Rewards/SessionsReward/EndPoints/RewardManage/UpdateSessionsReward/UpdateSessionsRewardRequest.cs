using Reward_Flow_v2.Common;

namespace RewardFlow_API.Rewards.SessionsReward.EndPoints.RewardManage.UpdateSessionsReward;

public record UpdateSessionsRewardRequest
{
    public Optional<string> RewardName;
    public Optional<string?> RewardCode;
    public Optional<short?> Year;
    public Optional<byte?> Term;
    public Optional<decimal> Percentage;
}