using FluentResults;
using Microsoft.EntityFrameworkCore;
using Reward_Flow_v2.Rewards.Data;
using RewardFlow_API.Rewards.Data;

namespace RewardFlow_API.Rewards.SessionsReward.Services;

public partial class SessionRewardService
{
    public async Task<Result<CourseAssignment?>> RemoveCourseAssignmentAsync(int courseAssignmentId)
    {
        await using var transaction = await dbcontext.Database.BeginTransactionAsync();

        try
        {
            var assignment = await dbcontext.CourseAssignment
                .FirstOrDefaultAsync(a => a.Id == courseAssignmentId);

            if (assignment is null)
                throw new ResultsException(
                    "Course assignment not found or doesn't exist.");

            var reward = await dbcontext.SessionRewardEntity
                .FirstOrDefaultAsync(r => r.Id == assignment.SessionRewardId);

            if (reward is null)
                throw new ResultsException(
                    "The session reward associated with the course assignment was not found.");

            var employeeIds = await dbcontext.CourseEmployee
                .Where(e => e.SubjectSessionRewardId == courseAssignmentId)
                .Select(e => e.EmployeeId)
                .Distinct()
                .ToArrayAsync();

            dbcontext.CourseAssignment.Remove(assignment);

            // CourseEmployee is cascade deleted here.
            // The following recalculation must see only the remaining assignments.
            await dbcontext.SaveChangesAsync();

            await RecalculateEmployeeRewardsAsync(
                reward,
                employeeIds);

            await dbcontext.SaveChangesAsync();
            await transaction.CommitAsync();

            return Result.Ok<CourseAssignment?>(null);
        }
        catch (ResultsException ex)
        {
            await transaction.RollbackAsync();
            return HandleFailureLogging(ex.Errors);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            logger.LogCritical(
                ex,
                "Failed to remove course assignment {CourseAssignmentId}",
                courseAssignmentId);

            return Result.Fail("Internal Consistency Error");
        }
    }
    
}