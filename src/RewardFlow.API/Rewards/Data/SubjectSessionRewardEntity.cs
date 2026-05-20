using FluentResults;
using Reward_Flow_v2.Rewards.Data.Database;
using RewardFlow_API.Common.Interface;
using System.Threading.Tasks;

namespace Reward_Flow_v2.Rewards.Data;

public class SubjectSessionRewardEntity : ITenantEntity
{
    private readonly List<EmployeeSessionSubject> _employeeSessionSubjects = new();
    
    public int Id { get; private init; }
    public Guid TenantId { get; set; }
    public int SessionRewardId { get; private set; }
    public int SemesterSubjectId { get; private set; }
    public Guid SubjectSnapshotId { get; private set; }
    public int NumberOfStudents { get; private set; }
    public int SessionCount { get; private set; }

    public int? MainEmployeeId { get; private set; }
    public int MaxNumberOfEmployees { get; set; }

    public virtual IReadOnlyCollection<EmployeeSessionSubject> EmployeeSessionSubject => _employeeSessionSubjects.AsReadOnly();
    public virtual SubjectSnapshot SubjectSnapshot { get; private set; } = null!;

    private SubjectSessionRewardEntity() { }

    private SubjectSessionRewardEntity(int sessionRewardId, int numberOfStudents, SubjectSnapshot subjectSnapshot,
        int? mainEmployeeId, int maxNumberOfEmployees)
    {
        SessionRewardId = sessionRewardId;
        MaxNumberOfEmployees = maxNumberOfEmployees;
        MainEmployeeId = mainEmployeeId;

        SubjectSnapshot = subjectSnapshot;
        SemesterSubjectId = subjectSnapshot.SemesterSubjectId;
        SubjectSnapshotId = subjectSnapshot.SnapshotId;

        UpdateNumberOfStudents(numberOfStudents);
    }

    public static Result<SubjectSessionRewardEntity> Create(int sessionRewardId, int numberOfStudents,
        SubjectSnapshot subjectSnapshot,
        int? mainEmployeeId = null, int maxNumberOfEmployees = 3)
    {
        if (subjectSnapshot is null)
            return Result.Fail<SubjectSessionRewardEntity>("Subject snapshot cannot be null.");

        if (maxNumberOfEmployees <= 0)
            return Result.Fail<SubjectSessionRewardEntity>("Max number of employees must be greater than zero.");

        var entity = new SubjectSessionRewardEntity(sessionRewardId, numberOfStudents, subjectSnapshot, mainEmployeeId,
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
