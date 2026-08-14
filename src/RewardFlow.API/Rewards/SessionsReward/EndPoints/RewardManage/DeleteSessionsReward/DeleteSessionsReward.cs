using Reward_Flow_v2.Common.EndpointValidation;
using Reward_Flow_v2.Rewards;
using Reward_Flow_v2.Rewards.Data.Database;

namespace RewardFlow_API.Rewards.SessionsReward.EndPoints.RewardManage.DeleteSessionsReward;

public static class DeleteSessionsReward
{
    public static void MapDeleteSessionsReward(this IEndpointRouteBuilder app)
    {
        app.MapDelete(RewardApiPath.SessionRewardsById, HandlerAsync)
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithTags(RewardApiPath.Tag)
            .ValidateAccess();
    }

    private static async Task<IResult> HandlerAsync(int rewardId, RewardDbContext dbContext)
    {
        try
        {
            var result = await dbContext.SessionRewardEntity.DeleteByKeyAsync(rewardId);

            return result == 0 ? Results.NotFound() : Results.NoContent();
        }
        catch (Exception e)
        {
            return Results.InternalServerError(e.Message);
        }
    }
}