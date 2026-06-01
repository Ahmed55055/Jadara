using Microsoft.EntityFrameworkCore;
using Reward_Flow_v2.Common;
using Reward_Flow_v2.Common.EndpointValidation;
using Reward_Flow_v2.Rewards.Common;
using Reward_Flow_v2.Rewards.SessionsReward.Interface;

namespace Reward_Flow_v2.Rewards.SessionsReward.EndPoints.UpdateSessionsReward;

public static class UpdateSessionsReward
{


    public static void MapUpdateSessionsReward(this IEndpointRouteBuilder app)
    {
        app.MapPatch(RewardApiPath.UpdateSessionsReward, HandlerAsync)
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