using Microsoft.EntityFrameworkCore;
using Reward_Flow_v2.Common.EndpointValidation;
using Reward_Flow_v2.Rewards;
using Reward_Flow_v2.Rewards.Data;
using Reward_Flow_v2.Rewards.Data.Database;

namespace RewardFlow_API.Rewards.SessionsReward.EndPoints.CourseAssignments.Get;

public static class GetAllCourseAssignments
{
    public static void MapGetAllCourseAssignments(this IEndpointRouteBuilder app)
    {
        app.MapGet(RewardApiPath.CourseAssignments, HandlerAsync)
            .RequireAuthorization()
            .Produces<List<CourseAssignmentDto>>(StatusCodes.Status200OK)
            .WithTags(RewardApiPath.Tag).ValidateAccess()
            .PreloadUser();
    }

    private static async Task<IResult> HandlerAsync(int rewardId, RewardDbContext dbContext, ILogger<CourseAssignment> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            var courseAssignments = await dbContext.CourseAssignment
                .AsNoTracking()
                .Include(c => c.StaffMembers)
                .Where(c => c.SessionRewardId == rewardId)
                .ToListAsync(cancellationToken);
            
            var result = courseAssignments
                .Select(c => c.MapResponseDto())
                .ToList();
            
            return Results.Ok(result);
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while getting course assignments");
            return Results.InternalServerError();
        }
    }
}