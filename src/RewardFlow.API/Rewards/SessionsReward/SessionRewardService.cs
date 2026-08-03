using FluentResults;
using Microsoft.EntityFrameworkCore;
using Reward_Flow_v2.Common.EmployeeLookup;
using Reward_Flow_v2.Employees.Data;
using Reward_Flow_v2.Rewards.Data;
using Reward_Flow_v2.Rewards.Data.Database;
using Reward_Flow_v2.Rewards.SessionsReward.Common;
using Reward_Flow_v2.Rewards.SessionsReward.Dtos;
using Reward_Flow_v2.Rewards.SessionsReward.Interface;
using RewardFlow_API.Rewards.Data;
using RewardFlow_API.Rewards.SessionsReward.EndPoints.Reward.CreateReward;
using Z.EntityFramework.Plus;

namespace Reward_Flow_v2.Rewards.SessionsReward;

public class SessionRewardService(
    RewardDbContext dbcontext,
    ISessionRewardCalculator calculator,
    ISessionRewardRules rules,
    ISnapshotService<TermCourse, CourseSnapshot> subjectSnapshotService,
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

    #region AssignEmployees

    private record RewardProcessingContext(
        SessionRewardEntity Reward,
        List<Employee> Employees,
        CourseSnapshot CourseSnapshot,
        List<EmployeeSnapshot> EmployeeSnapshots,
        CourseAssignment CourseAssignment,
        List<EmployeeSessions> EmployeeSessionRewards,
        List<EmployeeReward> EmployeeRewards,
        List<EmployeeSessionCount> EmployeeSessionCounts);


    private record EmployeeSessionCount(int EmployeeId, int TotalSessions);

    /// <summary>
    /// Assembles the complete processing context required for reward distribution.
    /// </summary>
    /// <remarks>
    /// Utilizes Entity Framework Future Queries to consolidate independent data fetches into 
    /// a single batch. Optimized to prevent N+1 degradation during high-volume processing.
    /// </remarks>
    /// <param name="employeesIds">A collection of unique employee identifiers.</param>
    /// <param name="rewardId">The target session reward identifier.</param>
    /// <param name="semesterSubjectId">The academic term subject identifier.</param>
    /// <returns>A successful result containing the populated mapping context, or a failure result if foundational entities are missing.</returns>

    private async Task<Result<RewardProcessingContext>> LoadProcessingContextAsync(int[] employeesIds, int rewardId,
        int semesterSubjectId)
    {
        // ==========================================
        //      PHASE ONE: Independent Bulking
        // ==========================================
        var employees = await employeeLookup.GetEmployeesAsync(employeesIds);

        var rewardQuery = dbcontext.SessionRewardEntity
            .DeferredFirstOrDefault(r => r.Id == rewardId)
            .FutureValue();

        var employeeRewardQuery = dbcontext.EmployeeReward
            .Where(e => employeesIds.Contains(e.EmployeeId))
            .Future();

        var subjectSemesterQuery = dbcontext.TermCourse
            .DeferredFirstOrDefault(s => s.Id == semesterSubjectId)
            .FutureValue();

        var employeeSnapshotQuery = employeeSnapshotService
            .GetLatestSnapshot(employeesIds)
            .Future();

        var employeeSessionRewardQuery = dbcontext.EmployeeSessions
            .Where(e => employeesIds.Contains(e.EmployeeId) && e.SessionRewardId == rewardId)
            .Future();

        // TODO:
        // Should take care of this entities indexes,
        // as this is one of the most used features in the system
        var empSessionsQuery = (
            from subjectSession in dbcontext.CourseAssignment.Where(s => s.SessionRewardId == rewardId)
            join empSessionSubject in dbcontext.CourseEmployee.Where(e => employeesIds.Contains(e.EmployeeId))
                on subjectSession.Id equals empSessionSubject.SubjectSessionRewardId
            group subjectSession.SessionCount by empSessionSubject.EmployeeId
            into groupResult
            select new EmployeeSessionCount(
                groupResult.Key,
                groupResult.Sum()
            )
        ).Future();

        var subjectSnapshotQuery = subjectSnapshotService.GetLatestSnapshot(semesterSubjectId).FutureValue();

        var subjectSessionQuery = dbcontext.CourseAssignment
            .DeferredFirstOrDefault(s =>
                s.SemesterSubjectId == semesterSubjectId && s.SessionRewardId == rewardId)
            .FutureValue();

        return new RewardProcessingContext(
            Reward: await rewardQuery.ValueAsync(),
            Employees: employees.ToList(),
            CourseSnapshot: subjectSnapshotQuery.Value,
            EmployeeSnapshots: employeeSnapshotQuery.SelectMany(e => e).ToList(),
            CourseAssignment: subjectSessionQuery.Value,
            EmployeeSessionRewards: employeeSessionRewardQuery.ToList(),
            EmployeeRewards: employeeRewardQuery.ToList(),
            EmployeeSessionCounts: empSessionsQuery.ToList()
        );
    }

    private static Result EnsureEmployeesRewards(int rewardId, int[] employeesIds,
        List<EmployeeReward> employeeRewards, List<EmployeeSnapshot> employeeSnapshots)
    {
        foreach (var employeeId in employeesIds)
        {
            if (employeeRewards.Any(e => e.EmployeeId == employeeId))
                continue;

            var newRecord = EmployeeReward.Create(rewardId, employeeSnapshots.First(e => e.EmployeeId == employeeId));

            employeeRewards.Add(newRecord);
        }

        return Result.Ok();
    }

    private static Result EnsureEmployeesSessionRewards(IEnumerable<Employee> employees,
        List<EmployeeSessions> employeeSessionRewards, int rewardId,
        List<EmployeeSnapshot> employeeSnapshots)
    {
        foreach (var employee in employees)
        {
            if (employeeSessionRewards.Any(e => e.EmployeeId == employee.EmployeeId))
                continue;

            var newRecord = EmployeeSessions.Create(rewardId,
                employeeSnapshots.First(e => e.EmployeeId == employee.EmployeeId));

            if (newRecord is null)
                return Result.Fail(
                    $"Failed to create EmployeeSessionReward for Employee {employee.EmployeeId} and Reward {rewardId}");

            employeeSessionRewards.Add(newRecord);
        }

        return Result.Ok();
    }

    private void EnsureEmployeeSnapshots(int[] employeesIds, List<EmployeeSnapshot> employeeSnapshots,
        IEnumerable<Employee> employees)
    {
        foreach (var employeesId in employeesIds)
        {
            if (!employeeSnapshots.Any(e => e.EmployeeId == employeesId))
                continue;

            var newRecord = employeeSnapshotService.Capture(employees.First(e => e.EmployeeId == employeesId));
            employeeSnapshots.Add(newRecord);
        }
    }

    private Result EnsureRequiredRecordsAsync(int[] employeesIds, RewardProcessingContext processingData)
    {
        EnsureEmployeeSnapshots(employeesIds, processingData.EmployeeSnapshots, processingData.Employees);

        var empSRResult = EnsureEmployeesSessionRewards(processingData.Employees, processingData.EmployeeSessionRewards,
            processingData.Reward.Id, processingData.EmployeeSnapshots);

        if (empSRResult.IsFailed)
            return empSRResult;


        var employeeRewardsResult = EnsureEmployeesRewards(processingData.Reward.Id, employeesIds,
            processingData.EmployeeRewards, processingData.EmployeeSnapshots);

        if (employeeRewardsResult.IsFailed)
            return employeeRewardsResult;

        EnusreEmployeeSessionCount(employeesIds, processingData.EmployeeSessionCounts);

        return Result.Ok();
    }

    private void EnusreEmployeeSessionCount(int[] employeesIds, List<EmployeeSessionCount> employeeSessionCounts)
    {
        foreach (var employeeId in employeesIds)
        {
            if (employeeSessionCounts.Any(e => e.EmployeeId == employeeId))
                continue;

            var newRecord = new EmployeeSessionCount(employeeId, 0);
            employeeSessionCounts.Add(newRecord);
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
        var reward =
            SessionRewardEntity.Create(dto.Year, dto.Semester, dto.Percentage, createdBy, dto.Name, dto.Code);
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

      public async Task<Result<CourseAssignment?>> AssignEmployeeAsync(AddCourseAssignmentDto dto)
    {
        await using var transaction = await dbcontext.Database.BeginTransactionAsync();
        try
        {
            var employeesIds = dto.EmployeesIds as int[]?? dto.EmployeesIds.ToArray();

            var loadingResult = await LoadProcessingContextAsync(employeesIds, dto.RewardId, dto.TermCourseId);
            if (loadingResult.IsFailed)
                return HandleFailureLogging(loadingResult.Errors);

            var ensureRequiredRecordsResult = EnsureRequiredRecordsAsync(employeesIds, loadingResult.Value);
            if (ensureRequiredRecordsResult.IsFailed)
                return HandleFailureLogging(ensureRequiredRecordsResult.Errors);

            var processingResult = ProcessAssignment(loadingResult.Value);
            if (processingResult.IsFailed)
                return HandleFailureLogging(processingResult.Errors);

            AddNewEntriesIntoDbContext(loadingResult.Value);

            await dbcontext.SaveChangesAsync();
            await transaction.CommitAsync();

            return Result.Ok<CourseAssignment?>(loadingResult.Value.CourseAssignment);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Assignment failed for Subject {SubjectId}", dto.TermCourseId);
            await transaction.RollbackAsync();
            return Result.Fail("Internal Consistency Error");
        }
    }

    private void AddNewEntriesIntoDbContext(RewardProcessingContext rewardContext)
    {
        foreach (var employeeSessionReward in rewardContext.EmployeeSessionRewards.Where(e => !IsTracked(e)))
        {
            dbcontext.Add(employeeSessionReward);
        }

        foreach (var employeeReward in rewardContext.EmployeeRewards.Where(e => !IsTracked(e)))
        {
            dbcontext.Add(employeeReward);
        }
    }

    private bool IsTracked<T>(T entry)
    {
        return dbcontext.Entry(entry).State != EntityState.Detached;
    }

    /// <summary>
    /// Executes the core reward distribution algorithms across the active assignment context.
    /// </summary>
    /// <remarks>
    /// Performs exclusively in-memory mutations on the provided context entities. Operates in O(N * M) time 
    /// complexity due to linear searches within nested employee collections. Relies on the external calculation service 
    /// for final financial allocations.
    /// </remarks>
    /// <param name="rewardContext">The populated processing context containing employee snapshots, session counts, and reward rules.</param>
    /// <returns>A success result if all updates are applied, or a failure result if the initial course assignment update fails.</returns>
    private Result ProcessAssignment(RewardProcessingContext rewardContext)
    {
        var result = rewardContext.CourseAssignment.UpdateEmployees(rewardContext.EmployeeSnapshots);

        if (result.IsFailed)
            return result;

        foreach (var emp in rewardContext.EmployeeSessionRewards)
        {
            var currentSessionCount = rewardContext.EmployeeSessionCounts.First(e => e.EmployeeId == emp.EmployeeId)
                .TotalSessions;

            var newSessionCount =
                rules.GetAllowedSessionCount(currentSessionCount + rewardContext.CourseAssignment.SessionCount);

            emp.UpdateSessionCount(newSessionCount);
        }

        foreach (var employeeReward in rewardContext.EmployeeRewards)
        {
            var empSessions = rewardContext.EmployeeSessionRewards.First(e => e.EmployeeId == employeeReward.EmployeeId)
                .SessionsCount;
            var salary = employeeReward.EmployeeSnapshot.Salary;
            var newTotal = calculator.CalculateTotal(empSessions, salary.Value, rewardContext.Reward.Percentage);

            employeeReward.UpdateAmount(newTotal);
        }

        return Result.Ok();
    }

    private Result<CourseAssignment?> HandleFailureLogging(IEnumerable<IError> errors)
    {
        if (logger.IsEnabled(LogLevel.Warning))
            logger.LogError("Failed to ensure required records. Errors: {Errors}", errors);
        return Result.Fail(errors);
    }

    #endregion


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
}