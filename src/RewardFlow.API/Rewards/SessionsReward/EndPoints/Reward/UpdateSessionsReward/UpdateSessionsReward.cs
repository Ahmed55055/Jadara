using Reward_Flow_v2.Common.EndpointValidation;
using Reward_Flow_v2.Rewards;

namespace RewardFlow_API.Rewards.SessionsReward.EndPoints.Reward.UpdateSessionsReward;

public static class UpdateSessionsReward
{


    public static void MapUpdateSessionsReward(this IEndpointRouteBuilder app)
    {
        app.MapPatch(RewardApiPath.SessionRewardsById, HandlerAsync)
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithTags(RewardApiPath.Tag)
            .Validation(new UpdateSessionsRewardValidator());
    }

    private static async Task<IResult> HandlerAsync(int id, UpdateSessionsRewardRequest request, IHttpContextAccessor httpContextAccessor, CancellationToken cancellationToken)
    {
        throw new System.NotImplementedException();
    }
}