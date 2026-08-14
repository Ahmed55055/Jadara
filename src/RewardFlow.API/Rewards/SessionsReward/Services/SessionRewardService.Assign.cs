using FluentResults;
using Microsoft.EntityFrameworkCore;
using Reward_Flow_v2.Employees.Data;
using Reward_Flow_v2.Rewards.Data;
using RewardFlow_API.Rewards.Data;
using RewardFlow_API.Rewards.SessionsReward.EndPoints.CourseAssignments.Create;
using RewardFlow_API.Rewards.SessionsReward.EndPoints.RewardManage.CreateReward;
using Z.EntityFramework.Plus;

namespace RewardFlow_API.Rewards.SessionsReward.Services;

public partial class SessionRewardService
{
    public class ResultsException : Exception
    {
        public IEnumerable<IError> Errors { get; }

        public ResultsException(IEnumerable<IError> errors)
        {
            this.Errors = errors;
        }

        public ResultsException(string errorMessage)
        {
            var error = new Error(errorMessage);

            Errors = [];
            Errors.Append(error);
        }
    }

    private class RewardProcessingContext
    {
        public SessionRewardEntity Reward { get; init; } = null!;
        public List<Employee> Employees { get; init; } = [];
        public CourseSnapshot? CourseSnapshot { get; init; }
        public Course Course { get; init; } = null!;
        public List<EmployeeSnapshot> EmployeeSnapshots { get; init; } = [];
        public List<EmployeeSessions> EmployeeSessionRewards { get; init; } = [];
        public List<EmployeeReward> EmployeeRewards { get; init; } = [];
        public List<EmployeeSessionCount> EmployeeSessionCounts { get; init; } = [];
        public CourseAssignment CourseAssignment { get; set; } = null!;
    }

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
    /// <param name="courseId">The academic term subject identifier.</param>
    /// <returns>A successful result containing the populated mapping context, or a failure result if foundational entities are missing.</returns>
    private async Task<Result<RewardProcessingContext>> LoadProcessingContextAsync(int[] employeesIds, int rewardId,
        int courseId)
    {
        // Immutable Entities
        var reward = await dbcontext.SessionRewardEntity
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == rewardId);

        if (reward is null) throw new ResultsException("Reward not found or doesn't exist");

        var employees = (await employeeLookup.GetEmployeesAsync(employeesIds)).ToList();

        if (employeesIds.Length > employees.Count) throw new ResultsException("Some employees where not found");

        // Creatable Only Entities
        var employeeSnapshotQuery = employeeSnapshotService
            .GetLatestSnapshot(employeesIds)
            .AsNoTracking()
            .Future();
        
        var courseSnapshotQuery = courseSnapshotService
            .GetLatestSnapshot(s => s.CourseId == courseId && s.Year == reward.Year && s.Term == reward.Term)
            .AsNoTracking()
            .FutureValue();

        var courseQuery = dbcontext.Course
            .Include(c => c.TermCourse.Where(t => t.Term == reward.Term && t.Year == reward.Year))
            .DeferredFirstOrDefault(c => c.Id == courseId)
            .FutureValue();
        
        // Creatable And Modifiable Entities
        var employeeRewardQuery = dbcontext.EmployeeReward
            .Where(e => e.RewardId == reward.Id && employeesIds.Contains(e.EmployeeId))
            .Future();

        var employeeSessionRewardQuery = dbcontext.EmployeeSessions
            .Where(e => employeesIds.AsEnumerable().Contains(e.EmployeeId) && e.SessionRewardId == reward.Id)
            .Future();


        // Additional information
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

        return new RewardProcessingContext
        {
            Reward = reward,
            Employees = employees,
            CourseSnapshot = await courseSnapshotQuery.ValueAsync(),
            Course = courseQuery.Value,
            EmployeeSnapshots = employeeSnapshotQuery.SelectMany(e => e).ToList(),
            EmployeeSessionRewards = employeeSessionRewardQuery.ToList(),
            EmployeeRewards = employeeRewardQuery.ToList(),
            EmployeeSessionCounts = empSessionsQuery.ToList()
        };
    }
    
    public async Task<Result<CourseAssignment?>> AssignEmployeeAsync(AddCourseAssignmentDto dto)
    {
        await using var transaction = await dbcontext.Database.BeginTransactionAsync();
        try
        {
            var employeesIds = dto.Employees.Select(e=>e.EmployeeId).ToArray();

            var loadingResult = await LoadProcessingContextAsync(employeesIds, dto.RewardId, dto.CourseId);

            UpdateCustomValues(loadingResult.Value, dto);
            
            EnsureRequiredRecords(employeesIds, loadingResult.Value, dto);

            ProcessAssignment(loadingResult.Value);

            AddNewEntriesIntoDbContext(loadingResult.Value);

            await dbcontext.SaveChangesAsync();
            await transaction.CommitAsync();

            return Result.Ok<CourseAssignment?>(loadingResult.Value.CourseAssignment);
        }
        catch (ResultsException ex)
        {
            return HandleFailureLogging(ex.Errors);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Assignment failed for Subject {SubjectId}", dto.CourseId);
            await transaction.RollbackAsync();
            return Result.Fail("Internal Consistency Error");
        }
    }

    private void UpdateCustomValues(RewardProcessingContext context, AddCourseAssignmentDto dto)
    {
        foreach (var employee in context.Employees)
        {
            var dtoEmployee = dto.Employees.First(e => e.EmployeeId == employee.Id);

            if (dtoEmployee.Salary is not null)
                employee.Salary = dtoEmployee.Salary;
        }

        var termCourse = context.Course.TermCourse.FirstOrDefault();

        if (termCourse is not null)
            termCourse.StudentCount = dto.StudentCount;
    }

    

    private void EnsureEmployeesRewards(int rewardId, int[] employeesIds, List<EmployeeReward> employeeRewards,
        List<EmployeeSnapshot> employeeSnapshots)
    {
        foreach (var employeeId in employeesIds)
        {
            if (employeeRewards.Any(e => e.EmployeeId == employeeId))
                continue;

            var empSnapshot = employeeSnapshots.First(s => s.EmployeeId == employeeId);
            var newRecord = EmployeeReward.Create(rewardId, empSnapshot);

            employeeRewards.Add(newRecord);
        }
    }

    private void EnsureEmployeesSessions(IEnumerable<Employee> employees, List<EmployeeSessions> employeeSessionRewards,
        int rewardId, List<EmployeeSnapshot> employeeSnapshots)
    {
        foreach (var employee in employees)
        {
            var empSessions = employeeSessionRewards.FirstOrDefault(s => s.EmployeeId == employee.Id);

            if (empSessions is not null)
                continue;

            var empSnapshot = employeeSnapshots.First(s => s.EmployeeId == employee.Id);
            var newRecord = EmployeeSessions.Create(rewardId, empSnapshot);

            employeeSessionRewards.Add(newRecord);
        }
    }

    private void EnsureEmployeeSnapshots(List<EmployeeSnapshot> employeeSnapshots, IEnumerable<Employee> employees)
    {
        foreach (var employee in employees)
        {
            var empSnapshot = employeeSnapshots.FirstOrDefault(s => s.EmployeeId == employee.Id);

            if (empSnapshot is not null && employeeSnapshotService.Compare(empSnapshot, employee))
                continue;

            var newRecord = employeeSnapshotService.Capture(employee);

            employeeSnapshots.Remove(empSnapshot);
            employeeSnapshots.Add(newRecord);
        }
    }

    private void EnsureRequiredRecords(int[] employeesIds, RewardProcessingContext processingData,
        AddCourseAssignmentDto dto)
    {
        EnsureEmployeeSnapshots(processingData.EmployeeSnapshots, processingData.Employees);

        EnsureEmployeesSessions(processingData.Employees, processingData.EmployeeSessionRewards,
            processingData.Reward.Id, processingData.EmployeeSnapshots);

        EnsureEmployeesRewards(processingData.Reward.Id, employeesIds, processingData.EmployeeRewards,
            processingData.EmployeeSnapshots);

        EnsureEmployeeSessionCount(employeesIds, processingData.EmployeeSessionCounts);

        CreateCourseAssignment(processingData, dto);
    }

    private void CreateCourseAssignment(RewardProcessingContext processingData, AddCourseAssignmentDto dto)
    {
        var snapshot = processingData.CourseSnapshot;

        var termCourse =
            processingData.Course.TermCourse.FirstOrDefault() ??
            TermCourse.Create(
                processingData.Course,
                processingData.Reward.Term!.Value,
                processingData.Reward.Year!.Value);

        termCourse.StudentCount = dto.StudentCount;
        
        if (!courseSnapshotService.Compare(snapshot, termCourse))
            snapshot = courseSnapshotService.Capture(termCourse);

        var assignEmployeeCount = dto.Employees.Count() <= 5
            ? dto.Employees.Count()
            : 5;

        var courseAssignment =
            CourseAssignment.Create(processingData.Reward.Id, snapshot, dto.MainEmployeeId, assignEmployeeCount);

        if (courseAssignment.IsFailed)
            throw new Exception($"Course assignment for course id: {termCourse.CourseId} could not be created.");

        processingData.CourseAssignment = courseAssignment.Value;
    }

    private void EnsureEmployeeSessionCount(int[] employeesIds, List<EmployeeSessionCount> employeeSessionCounts)
    {
        foreach (var employeeId in employeesIds)
        {
            if (employeeSessionCounts.Any(e => e.EmployeeId == employeeId))
                continue;

            var newRecord = new EmployeeSessionCount(employeeId, 0);
            employeeSessionCounts.Add(newRecord);
        }
    }

    private void AddNewEntriesIntoDbContext(RewardProcessingContext rewardContext)
    {
        var employeesSessions = rewardContext.EmployeeSessionRewards.Where(e => !IsTracked(e));
        var employeesRewards = rewardContext.EmployeeRewards.Where(e => !IsTracked(e));

        dbcontext.AddRange(employeesSessions);
        dbcontext.AddRange(employeesRewards);
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
    private void ProcessAssignment(RewardProcessingContext rewardContext)
    {
        var result = rewardContext.CourseAssignment.UpdateEmployees(rewardContext.EmployeeSnapshots);

        if (result.IsFailed) throw new ResultsException(result.Errors);

        UpdateSessionCount(rewardContext);
        UpdateEmployeesReward(rewardContext);
    }

    private void UpdateSessionCount(RewardProcessingContext rewardContext)
    {
        foreach (var emp in rewardContext.EmployeeSessionRewards)
        {
            var currentSessionCount = rewardContext
                .EmployeeSessionCounts
                .First(e => e.EmployeeId == emp.EmployeeId)
                .TotalSessions;

            var newSessionCount =
                rules.GetAllowedSessionCount(currentSessionCount + rewardContext.CourseAssignment.SessionCount);

            emp.UpdateSessionCount(newSessionCount);
        }
    }

    /*private async Task UpdateEmployeesReward(List<int> employeeIds, int rewardId)
    {
        var employeeRewards = dbcontext.EmployeeReward
            .Where(e => employeeIds.Contains(e.EmployeeId))
            .Future();

        var employeeSessions = dbcontext.EmployeeSessions
            .Where(e => employeeIds.Contains(e.EmployeeId) && e.SessionRewardId == rewardId)
            .Select(e => new
            {
                e.EmployeeId,
                e.SessionsCount
            })
            .Future();

       
    }*/
    private void UpdateEmployeesReward(RewardProcessingContext rewardContext)
    {
        foreach (var employeeReward in rewardContext.EmployeeRewards)
        {
            var empSessions = rewardContext.EmployeeSessionRewards.First(e => e.EmployeeId == employeeReward.EmployeeId)
                .SessionsCount;
            var salary = employeeReward.EmployeeSnapshot?.Salary;
            var newTotal = calculator.CalculateTotal(empSessions, salary.Value, rewardContext.Reward.Percentage);

            employeeReward.UpdateAmount(newTotal);
        }
    }

    private Result<CourseAssignment?> HandleFailureLogging(IEnumerable<IError> errors)
    {
        if (logger.IsEnabled(LogLevel.Warning))
            logger.LogError("Failed to ensure required records. Errors: {Errors}", errors);
        return Result.Fail(errors);
    }
}