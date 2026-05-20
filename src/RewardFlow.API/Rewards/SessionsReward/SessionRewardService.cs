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
using Z.EntityFramework.Plus;

namespace Reward_Flow_v2.Rewards.SessionsReward;

public class SessionRewardService(
    RewardDbContext dbcontext,
    ISessionRewardCalculator calculator,
    ISessionRewardRules rules,
    ISnapshotService<SemesterSubject, SubjectSnapshot> subjectSnapshotService,
    ISnapshotService<Employee, EmployeeSnapshot> employeeSnapshotService,
    IEmployeeLookupService employeeLookup,
    ILogger<SessionRewardService> logger) : ISessionReward
{
    public async Task<decimal> GetTotalAsync(int rewardId)
    {
        return await dbcontext.EmployeeReward
            .Where(er => er.RewardId == rewardId)
            .SumAsync(er => er.Total);
    }
    
    #region AssignEmployees
    private record RewardProcessingContext(
        SessionRewardEntity Reward,
        List<Employee> Employees,
        SubjectSnapshot SubjectSnapshot,
        List<EmployeeSnapshot> EmployeeSnapshots,
        SubjectSessionRewardEntity SubjectSessionReward,
        List<EmployeeSessionReward> EmployeeSessionRewards,
        List<EmployeeReward> EmployeeRewards,
        List<EmployeeSessionCount> EmployeeSessionCounts);

    

    private record EmployeeSessionCount(int EmployeeId, int TotalSessions);

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

        var subjectSemesterQuery = dbcontext.SubjectSemester
            .DeferredFirstOrDefault(s => s.Id == semesterSubjectId)
            .FutureValue();

        var employeeSnapshotQuery = employeeSnapshotService
            .GetLatestSnapshot(employeesIds)
            .Future();

        var employeeSessionRewardQuery = dbcontext.EmployeeSessionReward
            .Where(e => employeesIds.Contains(e.EmployeeId) && e.SessionRewardId == rewardId)
            .Future();

        // TODO:
        // Should take care of this entities indexes,
        // as this is one of the most used features in the system
        var empSessionsQuery = (
            from subjectSession in dbcontext.SubjectSessionRewardEntity.Where(s => s.SessionRewardId == rewardId)
            join empSessionSubject in dbcontext.EmployeeSessionSubject.Where(e => employeesIds.Contains(e.EmployeeId))
                on subjectSession.Id equals empSessionSubject.SubjectSessionRewardId
            group subjectSession.SessionCount by empSessionSubject.EmployeeId
            into groupResult
            select new EmployeeSessionCount(
                groupResult.Key,
                groupResult.Sum()
            )
        ).Future();

        var subjectSnapshotQuery = subjectSnapshotService.GetLatestSnapshot(semesterSubjectId).FutureValue();

        var subjectSessionQuery = dbcontext.SubjectSessionRewardEntity
            .DeferredFirstOrDefault(s =>
                s.SemesterSubjectId == semesterSubjectId && s.SessionRewardId == rewardId)
            .FutureValue();

        return new RewardProcessingContext(
            Reward: await rewardQuery.ValueAsync(),
            Employees: employees.ToList(),
            SubjectSnapshot: subjectSnapshotQuery.Value,
            EmployeeSnapshots: employeeSnapshotQuery.SelectMany(e => e).ToList(),
            SubjectSessionReward: subjectSessionQuery.Value,
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

            var newRecord = EmployeeReward.Create(rewardId, employeeId,
                employeeSnapshots.First(e => e.EmployeeId == employeeId));

            if (newRecord is null)
                return Result.Fail(
                    $"Failed to create EmployeeReward for Employee {employeeId} and Reward {rewardId}");

            employeeRewards.Add(newRecord);
        }

        return Result.Ok();
    }

    private static Result EnsureEmployeesSessionRewards(IEnumerable<Employee> employees,
        List<EmployeeSessionReward> employeeSessionRewards, int rewardId,
        List<EmployeeSnapshot> employeeSnapshots)
    {
        foreach (var employee in employees)
        {
            if (employeeSessionRewards.Any(e => e.EmployeeId == employee.EmployeeId))
                continue;

            var newRecord = EmployeeSessionReward.Create(rewardId,
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

    public async Task<Result<IEnumerable<EmployeeSessionReward>>> AssignEmployeeAsync(SessionSubjectDto dto)
    {
        await using var transaction = await dbcontext.Database.BeginTransactionAsync();
        try
        {
            var employeesIds = dto.Employees.Select(e => e.EmployeeId).Distinct().ToArray();

            var loadingResult = await LoadProcessingContextAsync(employeesIds, dto.RewardId, dto.SemesterSubjectId);
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

            return Result.Ok<IEnumerable<EmployeeSessionReward>>(loadingResult.Value.EmployeeSessionRewards);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Assignment failed for Subject {SubjectId}", dto.SemesterSubjectId);
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

    private Result ProcessAssignment(RewardProcessingContext rewardContext)
    {
        var result = rewardContext.SubjectSessionReward.UpdateEmployees(rewardContext.EmployeeSnapshots);

        if (result.IsFailed)
            return result;

        foreach (var emp in rewardContext.EmployeeSessionRewards)
        {
            var currentSessionCount = rewardContext.EmployeeSessionCounts.First(e => e.EmployeeId == emp.EmployeeId)
                .TotalSessions;

            var newSessionCount =
                rules.GetAllowedSessionCount(currentSessionCount + rewardContext.SubjectSessionReward.SessionCount);

            emp.UpdateSessionCount(newSessionCount);
        }

        foreach (var employeeReward in rewardContext.EmployeeRewards)
        {
            var empSessions = rewardContext.EmployeeSessionRewards.First(e => e.EmployeeId == employeeReward.EmployeeId)
                .SessionsCount;
            var salary = employeeReward.EmployeeSnapshot.Salary;
            var newTotal = calculator.CalculateTotal(empSessions, salary.Value, rewardContext.Reward.Percentage);

            employeeReward.UpdateTotal(newTotal);
        }

        return Result.Ok();
    }

    private Result<IEnumerable<EmployeeSessionReward>> HandleFailureLogging(IEnumerable<IError> errors)
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