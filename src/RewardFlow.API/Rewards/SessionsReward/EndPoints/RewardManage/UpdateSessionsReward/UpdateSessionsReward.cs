using Microsoft.EntityFrameworkCore;
using Reward_Flow_v2.Common.EndpointValidation;
using Reward_Flow_v2.Rewards;
using Reward_Flow_v2.Rewards.Data;
using Reward_Flow_v2.Rewards.Data.Database;

namespace RewardFlow_API.Rewards.SessionsReward.EndPoints.RewardManage.UpdateSessionsReward;

public static class UpdateSessionsReward
{
    public static void MapUpdateSessionsReward(this IEndpointRouteBuilder app)
    {
        app.MapPatch(RewardApiPath.SessionRewardsById, HandlerAsync)
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithTags(RewardApiPath.Tag)
            .Validation(new UpdateSessionsRewardValidator())
            .ValidateAccess();
    }

    private static async Task<IResult> HandlerAsync(int rewardId, UpdateSessionsRewardRequest request,
        RewardDbContext dbContext)
    {
        try
        {
            var entity = await dbContext.SessionRewardEntity
                .Include(s => s.Reward)
                .Where(s => s.Id == rewardId)
                .FirstOrDefaultAsync();

            if (entity is null)
                return Results.NotFound();

            UpdateFields(entity, request);

            await dbContext.SaveChangesAsync();

            return Results.NoContent();
        }
        catch (Exception e)
        {
            return Results.InternalServerError();
        }
    }

    private static void UpdateFields(SessionRewardEntity entity, UpdateSessionsRewardRequest request)
    {
        if (request.RewardName.HasValue)
            entity.Reward.Name = request.RewardName.Value;

        if (request.RewardCode.HasValue)
            entity.Reward.Code = request.RewardCode.Value;

        if (request.Year.HasValue)
            entity.Year = request.Year.Value;

        if (request.Term.HasValue)
            entity.Term = request.Term.Value;

        if (request.Percentage.HasValue)
            entity.Percentage = request.Percentage.Value;
    }
}