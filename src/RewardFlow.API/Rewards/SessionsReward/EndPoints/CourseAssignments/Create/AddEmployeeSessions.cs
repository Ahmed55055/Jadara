using Reward_Flow_v2.Common.EndpointValidation;
using Reward_Flow_v2.Rewards;
using Reward_Flow_v2.Rewards.SessionsReward;

namespace RewardFlow_API.Rewards.SessionsReward.EndPoints.CourseAssignments.Create;

public static class AddEmployeeSessions
{
    public static void MapAddCourseAssignments(this IEndpointRouteBuilder app)
    {
        app.MapPost(RewardApiPath.CourseAssignments, HandlerAsync)
            .RequireAuthorization()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags(RewardApiPath.Tag)
            .ValidateAccess();
    }

    private static async Task<IResult> HandlerAsync(int rewardId, AddCourseAssignmentDto dto, ISessionRewardService service, 
        CancellationToken cancellationToken)
    {
        var result = await service.AssignEmployeeAsync(dto);

        if (result.IsSuccess || result.Value == null)
            return Results.NoContent();

        return Results.CreatedAtRoute(RewardApiPath.CourseAssignments,
            new { Id = rewardId, CourseAssignmentId = result.Value.Id });
    }
}