using Microsoft.AspNetCore.Mvc;
using Reward_Flow_v2.Common;
using Reward_Flow_v2.Common.EndpointValidation;
using Reward_Flow_v2.Rewards.Data.Database;
using RewardFlow_API.Rewards.SessionsReward.EndPoints.CourseAssignments.Get;
using RewardFlow_API.Rewards.SessionsReward.Services;

namespace Reward_Flow_v2.Rewards.SessionsReward.EndPoints.CourseAssignments.Remove;

public static class RemoveCourseAssignment
{
    public static void MapRemoveCourseAssignment(this IEndpointRouteBuilder app)
    {
        app.MapDelete(RewardApiPath.CourseAssignmentsById, HandlerAsync)
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags(RewardApiPath.Tag)
            .ValidateAccess()
            .PreloadUser();
    }

    private static async Task<IResult> HandlerAsync(int rewardId, int courseAssignmentId, ISessionRewardService service, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}