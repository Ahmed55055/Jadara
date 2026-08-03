using RewardFlow_API.Rewards.SessionsReward.EndPoints.Reward.CreateReward;
using RewardFlow_API.Rewards.SessionsReward.EndPoints.Reward.DeleteSessionsReward;
using RewardFlow_API.Rewards.SessionsReward.EndPoints.Reward.GetAllSessionsRewards;
using RewardFlow_API.Rewards.SessionsReward.EndPoints.Reward.GetSessionsRewardById;
using RewardFlow_API.Rewards.SessionsReward.EndPoints.Reward.UpdateSessionsReward;
using RewardFlow_API.Rewards.SessionsReward.EndPoints.SessionReward.CourseAssignments;

namespace Reward_Flow_v2.Rewards.SessionsReward.EndPoints;

public static class SessionRewardEndpoints
{
    public static void MapSessionRewardEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapCreateSessionsReward();
        app.MapGetAllSessionsRewards();
        app.MapGetSessionsRewardById();
        app.MapUpdateSessionsReward();
        app.MapDeleteSessionsReward();
        app.MapAddEmployeeSessions();
        app.MapGetEmployeeSessions();
    }

}