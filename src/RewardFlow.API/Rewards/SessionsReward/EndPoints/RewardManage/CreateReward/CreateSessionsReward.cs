using Reward_Flow_v2.Common;
using Reward_Flow_v2.Common.EndpointValidation;
using Reward_Flow_v2.Rewards;
using Reward_Flow_v2.Rewards.SessionsReward;
using Reward_Flow_v2.Rewards.SessionsReward.Dtos;

namespace RewardFlow_API.Rewards.SessionsReward.EndPoints.RewardManage.CreateReward;

public static partial class CreateSessionsReward
{
    public record Request(string? Name, string? Code, short? Year, byte? Semester, decimal Percentage);

    public static void MapCreateSessionsReward(this IEndpointRouteBuilder app)
    {
        app.MapPost(RewardApiPath.SessionRewards, HandlerAsync)
            .RequireAuthorization()
            .Produces<SessionRewardDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithTags(RewardApiPath.Tag)
            .Validation(new CreateRewardRequestValidator());
    }

    private static async Task<IResult> HandlerAsync(Request request, ISessionRewardService sessionRewardService , IHttpContextAccessor httpContextAccessor, CancellationToken cancellationToken)
    {
        var currentUserId = await httpContextAccessor.GetCurrentUserIntIdAsync(cancellationToken);
        
        var rewardResult = await sessionRewardService.CreateReward(request, currentUserId);
        
        return rewardResult.IsSuccess?
             Results.Created(RewardApiPath.SessionRewardsById, rewardResult.Value):
             Results.InternalServerError();
    }
}