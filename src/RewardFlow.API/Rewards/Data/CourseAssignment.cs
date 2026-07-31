using FluentResults;
using Reward_Flow_v2.Rewards.Data.Database;
using RewardFlow_API.Common.Interface;
using RewardFlow_API.Rewards.Data;
using System.Threading.Tasks;

namespace Reward_Flow_v2.Rewards.Data;

public sealed class CourseAssignment : ITenantEntity
{
    private readonly List<CourseEmployee> _employeeSessionSubjects = new();
    
    public int Id { get; private init; }
    public Guid TenantId { get; set; }
    public int SessionRewardId { get; private set; }
    public int SemesterSubjectId { get; private set; }
    public Guid SubjectSnapshotId { get; private set; }
    public int NumberOfStudents { get; private set; }
    public int SessionCount { get; private set; }

    public int? MainEmployeeId { get; private set; }
    public int MaxNumberOfEmployees { get; set; }

    public IReadOnlyCollection<CourseEmployee> StaffMembers => _employeeSessionSubjects.AsReadOnly();
    public CourseSnapshot CourseSnapshot { get; private set; } = null!;

    private CourseAssignment() { }

    private CourseAssignment(int sessionRewardId, int numberOfStudents, CourseSnapshot courseSnapshot,
        int? mainEmployeeId, int maxNumberOfEmployees)
    {
        SessionRewardId = sessionRewardId;
        MaxNumberOfEmployees = maxNumberOfEmployees;
        MainEmployeeId = mainEmployeeId;

        CourseSnapshot = courseSnapshot;
        SemesterSubjectId = courseSnapshot.SemesterSubjectId;
        SubjectSnapshotId = courseSnapshot.SnapshotId;

        UpdateNumberOfStudents(numberOfStudents);
    }

    public static Result<CourseAssignment> Create(int sessionRewardId, int numberOfStudents,
        CourseSnapshot courseSnapshot,
        int? mainEmployeeId = null, int maxNumberOfEmployees = 3)
    {
        if (courseSnapshot is null)
            return Result.Fail<CourseAssignment>("Subject snapshot cannot be null.");

        if (maxNumberOfEmployees <= 0)
            return Result.Fail<CourseAssignment>("Max number of employees must be greater than zero.");

        var entity = new CourseAssignment(sessionRewardId, numberOfStudents, courseSnapshot, mainEmployeeId,
            maxNumberOfEmployees);
        return Result.Ok(entity);
    }

    /// <summary>
    /// Updates the employees for this subject session reward, removing any that are not in the incoming collection
    /// </summary>
    /// <param name="employeeSnapshot">employee snapshot to update with</param>
    /// <returns><c>Result.Ok()</c> if successful, otherwise <c>Result.Fail()</c></returns>
    public Result UpdateEmployees(IEnumerable<EmployeeSnapshot> employeeSnapshot)
    {
        var employeeSnapshots = employeeSnapshot as EmployeeSnapshot[] ?? employeeSnapshot.ToArray();
        employeeSnapshots = employeeSnapshots.DistinctBy(s=>s.EmployeeId).ToArray();

        var count = employeeSnapshots.Length;

        if (count > MaxNumberOfEmployees)
            return Result.Fail(
                $"Exceeding maximum number of employees Added: {count}. Max Allowed: {MaxNumberOfEmployees}");

        if (count <= 0)
            return Result.Fail($"No employees added");
        
        var incomingSnapshotsIds = employeeSnapshots.Select(e => e.SnapshotId).ToArray();
        var outComing = _employeeSessionSubjects
            .Where(e => !incomingSnapshotsIds.Contains(e.EmployeeSnapshotId))
            .ToList();

        foreach (var removeObject in outComing)
            _employeeSessionSubjects.Remove(removeObject);

        foreach (var snapshot in employeeSnapshots)
        {
            var isExists = _employeeSessionSubjects.Any(e => e.EmployeeSnapshotId == snapshot.SnapshotId);

            if (isExists)
                continue;

            _employeeSessionSubjects.Add(CreateEmployeeSessionSubject(snapshot));
        }

        return Result.Ok();
    }

    /// <summary>
    /// Updates main employee, employee session subject must be related to this subject session reward
    /// </summary>
    /// <param name="courseEmployee">employee session subject that is related to this subject session reward</param>
    /// <returns> <c>Result.Ok()</c> if successful, otherwise <c>Result.Fail()</c> </returns>
    public Result UpdateMainEmployee(CourseEmployee courseEmployee)
    {
        if (courseEmployee is null)
            return Result.Fail("Employee session subject is null");

        if (!_employeeSessionSubjects.Contains(courseEmployee))
            return Result.Fail("Main employee not found");

        MainEmployeeId = courseEmployee.EmployeeId;
        return Result.Ok();
    }

    private CourseEmployee CreateEmployeeSessionSubject(EmployeeSnapshot employeeSnapshot)
    {
        return new CourseEmployee(course : this,employeeSnapshot : employeeSnapshot) ;
    }

    /// <summary>
    /// Updates the subject snapshot for this subject session reward entity
    /// </summary>
    /// <param name="courseSnapshot">subject snapshot to update with</param>
    /// <returns><c>Result.Ok()</c> if successful, otherwise <c>Result.Fail()</c></returns>
    public Result UpdateSubject(CourseSnapshot courseSnapshot)
    {
        if (courseSnapshot is null)
            return Result.Fail($"Subject is null");

        CourseSnapshot = courseSnapshot;
        SemesterSubjectId = courseSnapshot.SemesterSubjectId;

        return Result.Ok();
    }

    public void UpdateNumberOfStudents(int numberOfStudents)
    {
        NumberOfStudents = numberOfStudents;
        SessionCount = CalculateSessions(numberOfStudents);
    }

    private int CalculateSessions(int studentsCount)
    {
        return studentsCount switch
        {
            < 1 => 0,
            < 5 => 1,
            _ => (int)Math.Round(studentsCount / 5.0)
        };
    }
}