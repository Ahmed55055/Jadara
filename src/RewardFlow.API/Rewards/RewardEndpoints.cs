using RewardFlow_API.Rewards.SessionsReward.EndPoints.Reward.CreateReward;
using RewardFlow_API.Rewards.SessionsReward.EndPoints.Reward.DeleteSessionsReward;
using RewardFlow_API.Rewards.SessionsReward.EndPoints.Reward.GetAllSessionsRewards;
using RewardFlow_API.Rewards.SessionsReward.EndPoints.Reward.GetSessionsRewardById;
using RewardFlow_API.Rewards.SessionsReward.EndPoints.Reward.UpdateSessionsReward;
using RewardFlow_API.Rewards.SessionsReward.EndPoints.SessionReward.CourseAssignments;

namespace Reward_Flow_v2.Rewards;

public static class RewardEndpoints
{
    public static void MapRewardEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapCreateSessionsReward();
        app.MapGetAllSessionsRewards();
        app.MapGetSessionsRewardById();
        app.MapUpdateSessionsReward();
        app.MapDeleteSessionsReward();
        app.MapAddEmployeeSessions();
    }
}