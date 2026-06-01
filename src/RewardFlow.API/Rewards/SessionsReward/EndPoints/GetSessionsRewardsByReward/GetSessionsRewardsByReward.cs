using Microsoft.EntityFrameworkCore;
using Reward_Flow_v2.Common;
using Reward_Flow_v2.Rewards.SessionsReward.Dtos;
using Reward_Flow_v2.Rewards.SessionsReward.Interface;

namespace Reward_Flow_v2.Rewards.SessionsReward.EndPoints.GetSessionsRewardsByReward;

public static class GetSessionsRewardsByReward
{
    public static void MapGetSessionsRewardsByRewardId(this IEndpointRouteBuilder app)
    {
        app.MapGet(RewardApiPath.GetSessionsRewardsByRewardId, HandlerAsync)
            .RequireAuthorization()
            .Produces<SessionRewardDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithTags(RewardApiPath.Tag);
    }

    private static async Task<IResult> HandlerAsync(int rewardId, IHttpContextAccessor httpContextAccessor, CancellationToken cancellationToken)
    {
        throw new System.NotImplementedException();
    }
}