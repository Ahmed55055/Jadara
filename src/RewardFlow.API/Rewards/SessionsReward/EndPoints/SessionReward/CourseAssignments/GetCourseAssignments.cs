using Microsoft.EntityFrameworkCore;
using Reward_Flow_v2.Common;
using Reward_Flow_v2.Common.EndpointValidation;
using Reward_Flow_v2.Rewards;
using Reward_Flow_v2.Rewards.Data;
using Reward_Flow_v2.Rewards.Data.Database;

namespace RewardFlow_API.Rewards.SessionsReward.EndPoints.SessionReward.CourseAssignments;

public static class GetCourseAssignments
{
    public static void MapGetEmployeeSessions(this IEndpointRouteBuilder app)
    {
        app.MapPost(RewardApiPath.CourseAssignmentsById, HandlerAsync)
            .RequireAuthorization()
            .Produces(StatusCodes.Status200OK, typeof(CourseAssignmentDto))
            .Produces(StatusCodes.Status404NotFound)
            .WithTags(RewardApiPath.Tag)
            .ValidateAccess()
            .PreloadUser();
    }

    private static async Task<IResult> HandlerAsync(int rewardId, int courseAssignmentId, ScopedUserContext scopedUserContext,
        RewardDbContext dbcontext, IHttpContextAccessor httpContextAccessor, ILogger logger,
        CancellationToken cancellationToken)
    {
        try
        {
            var courseAssignment = await dbcontext.CourseAssignment
                .Include(s => s.StaffMembers)
                .FirstOrDefaultAsync(c => c.Id == courseAssignmentId, cancellationToken: cancellationToken);

            return courseAssignment is null
                ? Results.NotFound()
                : Results.Ok(MapDto(courseAssignment));
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while executing the request");
            return Results.InternalServerError();
        }
    }

    private static CourseAssignmentDto MapDto(CourseAssignment assignment)
    {
        var employees = assignment.StaffMembers
            .Select(e => new AssignedEmployeesDto(e.EmployeeId))
            .ToList();

        return new CourseAssignmentDto(
            assignment.Id,
            assignment.SessionRewardId,
            assignment.SemesterSubjectId,
            assignment.SessionCount,
            employees);
    }
}

public record CourseAssignmentDto(
    int CourseAssignmentId,
    int RewardId,
    int TermCourseId,
    int SessionsCount,
    List<AssignedEmployeesDto> AssignedEmployees);

public record AssignedEmployeesDto(int EmployeeId);