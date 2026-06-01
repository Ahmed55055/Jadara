using Microsoft.EntityFrameworkCore;
using Reward_Flow_v2.Common;
using Reward_Flow_v2.Rewards.Data.Database;

namespace Reward_Flow_v2.Rewards.SessionsReward.EndPoints.DeleteSessionsReward;

public static class DeleteSessionsReward
{
    public static void MapDeleteSessionsReward(this IEndpointRouteBuilder app)
    {
        app.MapDelete(RewardApiPath.DeleteSessionsReward, HandlerAsync)
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithTags(RewardApiPath.Tag);
    }

    private static async Task<IResult> HandlerAsync(int id, RewardDbContext dbContext, IHttpContextAccessor httpContextAccessor, CancellationToken cancellationToken)
    {
        throw new System.NotImplementedException();
    }
}