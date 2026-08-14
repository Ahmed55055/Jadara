using FluentResults;
using Microsoft.EntityFrameworkCore;
using Reward_Flow_v2.Common.EmployeeLookup;
using Reward_Flow_v2.Employees.Data;
using Reward_Flow_v2.Rewards;
using Reward_Flow_v2.Rewards.Data;
using Reward_Flow_v2.Rewards.Data.Database;
using Reward_Flow_v2.Rewards.SessionsReward;
using Reward_Flow_v2.Rewards.SessionsReward.Common;
using Reward_Flow_v2.Rewards.SessionsReward.Interface;
using RewardFlow_API.Rewards.Data;
using RewardFlow_API.Rewards.SessionsReward.EndPoints.CourseAssignments.Create;
using RewardFlow_API.Rewards.SessionsReward.EndPoints.RewardManage.CreateReward;
using Z.EntityFramework.Plus;

namespace RewardFlow_API.Rewards.SessionsReward.Services;

public partial class SessionRewardService(
    RewardDbContext dbcontext,
    ISessionRewardCalculator calculator,
    ISessionRewardRules rules,
    ISnapshotService<TermCourse, CourseSnapshot> courseSnapshotService,
    ISnapshotService<Employee, EmployeeSnapshot> employeeSnapshotService,
    IEmployeeLookupService employeeLookup,
    ILogger<SessionRewardService> logger) : ISessionRewardService
{
    public async Task<decimal> GetTotalAsync(int rewardId)
    {
        return await dbcontext.EmployeeReward
            .Where(er => er.RewardId == rewardId)
            .SumAsync(er => er.Amount);
    }

    private async Task RecalculateEmployeeRewardsAsync(
        SessionRewardEntity reward,
        int[] employeeIds)
    {
        var employeeSessionCounts = await (
            from courseAssignment in dbcontext.CourseAssignment
            join courseEmployee in dbcontext.CourseEmployee
                on courseAssignment.Id equals courseEmployee.SubjectSessionRewardId
            where courseAssignment.SessionRewardId == reward.Id
                  && employeeIds.Contains(courseEmployee.EmployeeId)
            group courseAssignment.SessionCount by courseEmployee.EmployeeId
            into groupResult
            select new EmployeeSessionCount(
                groupResult.Key,
                groupResult.Sum())
        ).ToListAsync();

        var employeeSessions = await dbcontext.EmployeeSessions
            .Where(e =>
                e.SessionRewardId == reward.Id &&
                employeeIds.Contains(e.EmployeeId))
            .ToListAsync();

        var employeeRewards = await dbcontext.EmployeeReward
            .Include(e => e.EmployeeSnapshot)
            .Where(e =>
                e.RewardId == reward.Id &&
                employeeIds.Contains(e.EmployeeId))
            .ToListAsync();

        foreach (var employeeSession in employeeSessions)
        {
            var sessionCount = employeeSessionCounts
                .FirstOrDefault(e => e.EmployeeId == employeeSession.EmployeeId);

            if (sessionCount is null)
            {
                dbcontext.EmployeeSessions.Remove(employeeSession);
                continue;
            }

            var allowedSessionCount =
                rules.GetAllowedSessionCount(sessionCount.TotalSessions);

            employeeSession.UpdateSessionCount(allowedSessionCount);
        }

        foreach (var employeeReward in employeeRewards)
        {
            var employeeSession = employeeSessions
                .FirstOrDefault(e => e.EmployeeId == employeeReward.EmployeeId);

            if (employeeSession is null ||
                dbcontext.Entry(employeeSession).State == EntityState.Deleted)
            {
                dbcontext.EmployeeReward.Remove(employeeReward);
                continue;
            }

            var salary = employeeReward.EmployeeSnapshot.Salary;

            var total = calculator.CalculateTotal(
                employeeSession.SessionsCount,
                salary.Value,
                reward.Percentage);

            employeeReward.UpdateAmount(total);
        }
    }

    /// <summary>
    /// Creates a new session reward entity and persists it to the database.
    /// </summary>
    /// <param name="dto">The request data containing reward details including year, semester, percentage, name, and code.</param>
    /// <param name="createdBy">The identifier of the user creating the reward.</param>
    /// <returns>A success result containing the new reward identifier, or a failure result if persistence fails.</returns>
    public async Task<Result<int>> CreateReward(CreateSessionsReward.Request dto, int createdBy)
    {
        var reward = SessionRewardEntity.Create(dto.Year, dto.Semester, dto.Percentage, createdBy, dto.Name, dto.Code);
        try
        {
            dbcontext.Add(reward);
            await dbcontext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to create new Session Reward");
        }

        return reward.Id == 0
            ? Result.Fail("Couldn't create this reward")
            : Result.Ok(reward.Id);
    }

    public Task<Result<EmployeeRewardDto?>> GetEmployeeReward(int employeeId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<EmployeeRewardDto>> GetEmployeesRewards()
    {
        throw new NotImplementedException();
    }

    public Task<decimal> GetTotal()
    {
        throw new NotImplementedException();
    }

    public void RemoveCourseAssignment(int rewardId, int courseAssignmentId)
    {
        throw new NotImplementedException();
    }
}