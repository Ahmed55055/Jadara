using Microsoft.EntityFrameworkCore;
using Reward_Flow_v2.Common;
using Reward_Flow_v2.Rewards.Data.Database;
using Reward_Flow_v2.Rewards.SessionsReward.Dtos;

namespace Reward_Flow_v2.Rewards.SessionsReward.EndPoints.GetAllSessionsRewards;

public static class GetAllSessionsRewards
{
    public static void MapGetAllSessionsRewards(this IEndpointRouteBuilder app)
    {
        app.MapGet(RewardApiPath.GetAllSessionsRewards, HandlerAsync)
            .RequireAuthorization()
            .Produces<IEnumerable<SessionRewardDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithTags(RewardApiPath.Tag);
    }

    private static async Task<IResult> HandlerAsync(int limit, RewardDbContext dbContext, IHttpContextAccessor httpContextAccessor, CancellationToken cancellationToken)
    {
        var currentUserId = await httpContextAccessor.GetCurrentUserIntIdAsync(cancellationToken);
        
        if (currentUserId == 0)
            return Results.Unauthorized();

        try
        {
            var sessionsRewards = await dbContext.SessionRewardEntity
                .Include(sr => sr.Reward)
                .Where(sr => sr.Reward.CreatedBy == currentUserId)
                .Take(limit > 0 ? limit : 100)
                .Select(sr => new SessionRewardDto(
                    sr.Id,
                    sr.Reward.Name,
                    sr.Reward.Code,
                    sr.Year,
                    sr.semester,
                    sr.Percentage,
                    0,
                    currentUserId
                ))
                .ToListAsync(cancellationToken);

            return Results.Ok(sessionsRewards);
        }
        catch (Exception)
        {
            return Results.InternalServerError();
        }
    }
}