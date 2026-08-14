using Microsoft.EntityFrameworkCore;
using Reward_Flow_v2.Common;
using Reward_Flow_v2.Common.EndpointValidation;
using Reward_Flow_v2.Rewards;
using Reward_Flow_v2.Rewards.Data;
using Reward_Flow_v2.Rewards.Data.Database;

namespace RewardFlow_API.Rewards.SessionsReward.EndPoints.CourseAssignments.Get;

public static class GetCourseAssignmentsById
{
    public static void MapGetCourseAssignment(this IEndpointRouteBuilder app)
    {
        app.MapGet(RewardApiPath.CourseAssignmentsById, HandlerAsync)
            .RequireAuthorization()
            .Produces<CourseAssignmentDto>()
            .Produces(StatusCodes.Status404NotFound)
            .WithTags(RewardApiPath.Tag)
            .ValidateAccess()
            .PreloadUser();
    }

    private static async Task<IResult> HandlerAsync(int rewardId, int courseAssignmentId, RewardDbContext dbcontext,
         ILogger<CourseAssignment> logger, CancellationToken cancellationToken)
    {
        try
        {
            var courseAssignment = await dbcontext.CourseAssignment
                .Include(s => s.StaffMembers)
                .FirstOrDefaultAsync(c => c.Id == courseAssignmentId, cancellationToken: cancellationToken);

            return courseAssignment is null
                ? Results.NotFound()
                : Results.Ok(courseAssignment.MapResponseDto());
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while executing the request");
            return Results.InternalServerError();
        }
    }
}