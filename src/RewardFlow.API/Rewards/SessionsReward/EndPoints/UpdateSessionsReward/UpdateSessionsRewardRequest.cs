using System.ComponentModel.DataAnnotations;
using Reward_Flow_v2.Common;

namespace Reward_Flow_v2.Rewards.SessionsReward.EndPoints.UpdateSessionsReward;

public record UpdateSessionsRewardRequest
{
    [Required] public int SemesterSubjectId;
    public Optional<string> RewardName;
    public Optional<string?> RewardCode;
    public Optional<int?> Year;
    public Optional<byte?> Semester;
    public Optional<decimal> Percentage;
}