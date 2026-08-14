using Reward_Flow_v2.Common.EndpointValidation;
using RewardFlow_API.Rewards.SessionsReward.EndPoints.CourseAssignments.Create;
using RewardFlow_API.Rewards.SessionsReward.EndPoints.CourseAssignments.Get;
using RewardFlow_API.Rewards.SessionsReward.Services;

namespace Reward_Flow_v2.Rewards.SessionsReward.EndPoints.CourseAssignments.Update;

public static class UpdateCourseAssignment
{
    public static void MapUpdateCourseAssignment(this IEndpointRouteBuilder app)
    {
        app.MapPut(RewardApiPath.CourseAssignmentsById, HandlerAsync)
            .RequireAuthorization()
            .Produces<CourseAssignmentDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags(RewardApiPath.Tag)
            .ValidateAccess()
            .PreloadUser();
    }

    private static async Task<IResult> HandlerAsync(int rewardId, int courseAssignmentId, UpdateCourseAssignmentDto dto,
        ISessionRewardService service, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}

public record UpdateCourseAssignmentDto
{
    public int? StudentCount { get; init; }
    public required int MainEmployeeId { get; init; }
    public required IEnumerable<AssignEmployeeDto> Employees { get; init; }
}