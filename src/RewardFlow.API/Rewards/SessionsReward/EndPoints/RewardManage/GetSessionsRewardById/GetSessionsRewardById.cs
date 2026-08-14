using Microsoft.EntityFrameworkCore;
using Reward_Flow_v2.Common.EndpointValidation;
using Reward_Flow_v2.Rewards;
using Reward_Flow_v2.Rewards.Data;
using Reward_Flow_v2.Rewards.Data.Database;
using Reward_Flow_v2.Rewards.SessionsReward.Dtos;

namespace RewardFlow_API.Rewards.SessionsReward.EndPoints.RewardManage.GetSessionsRewardById;

public static class GetSessionsRewardById
{
    public static void MapGetSessionsRewardById(this IEndpointRouteBuilder app)
    {
        app.MapGet(RewardApiPath.SessionRewardsById, HandlerAsync)
            .RequireAuthorization()
            .Produces<SessionRewardDto>()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithTags(RewardApiPath.Tag)
            .ValidateAccess();
    }

    private static async Task<IResult> HandlerAsync(int rewardId, RewardDbContext dbContext, CancellationToken cancellationToken)
    {
        var result = await dbContext.SessionRewardEntity
            .AsNoTracking()
            .Include(s=>s.Reward)
            .Where(r => r.Id == rewardId)
            .Select(s => MapSessionReward(s))
            .FirstOrDefaultAsync(cancellationToken);

        return result is null ? Results.NotFound() : Results.Ok(result);
    }

    private static SessionRewardDto MapSessionReward(SessionRewardEntity sessionReward)
    {
        return new SessionRewardDto(sessionReward.Id, sessionReward.Reward.Name, sessionReward.Reward.Code,
            sessionReward.Year, sessionReward.Term, sessionReward.Percentage, sessionReward.Reward.Total);
    }
}