using RewardFlow_API.Rewards.SessionsReward.EndPoints.CourseAssignments.Create;
using RewardFlow_API.Rewards.SessionsReward.EndPoints.RewardManage.CreateReward;
using RewardFlow_API.Rewards.SessionsReward.EndPoints.RewardManage.DeleteSessionsReward;
using RewardFlow_API.Rewards.SessionsReward.EndPoints.RewardManage.GetAllSessionsRewards;
using RewardFlow_API.Rewards.SessionsReward.EndPoints.RewardManage.GetSessionsRewardById;
using RewardFlow_API.Rewards.SessionsReward.EndPoints.RewardManage.UpdateSessionsReward;

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
    }
}