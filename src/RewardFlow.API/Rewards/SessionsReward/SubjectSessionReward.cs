using FluentResults;
using Microsoft.EntityFrameworkCore;
using Reward_Flow_v2.Common;
using Reward_Flow_v2.Rewards.Data;
using Reward_Flow_v2.Rewards.Data.Database;
using System.Threading.Tasks;

namespace Reward_Flow_v2.Rewards.SessionsReward;

public sealed class SubjectSessionReward
{
    public int Id { get => Entity.Id; }
    public int SessionRewardId { get => Entity.SessionRewardId; set => Entity.SessionRewardId = value; }
    public int NumberOfSessions { get => Entity.NumberOfSessions; private set => Entity.NumberOfSessions = value; }
    public int SubjectId { get => Entity.SemesterSubjectId; set => Entity.SemesterSubjectId = value; }
    public int StudentsNumber { get => Entity.StudentsNumber; set => Entity.StudentsNumber = value; }
    public int? MainEmployeeId { get => Entity.MainEmployeeId; set => Entity.MainEmployeeId = value; }
    public int MaxNumberOfEmployees { get => Entity.MaxNumberOfEmployees; set => Entity.MaxNumberOfEmployees = value; }

    private SubjectSessionRewardEntity Entity { get; set; }
    private readonly RewardDbContext _dbContext;

    public SubjectSessionReward(int sessionRewardId, byte numberOfSessions, int subjectId, int studentsNumber, int mainEmployeeId, RewardDbContext dbContext, byte maxNumberOfEmployees = 3)
    {
        Entity = new SubjectSessionRewardEntity();
        SessionRewardId = sessionRewardId;
        NumberOfSessions = numberOfSessions;
        SubjectId = subjectId;
        StudentsNumber = studentsNumber;
        MainEmployeeId = mainEmployeeId;
        MaxNumberOfEmployees = maxNumberOfEmployees;
        _dbContext = dbContext;
    }

    private SubjectSessionReward(SubjectSessionRewardEntity subjectSessionRewardEntity, RewardDbContext dbContext)
    {
        Entity = subjectSessionRewardEntity;
        _dbContext = dbContext;
    }

    public static SubjectSessionReward Find(int subjectSessionRewardId, RewardDbContext dbContext)
    {
        var subjectSessionReward =
            dbContext.SubjectSessionRewardEntity
            .Include(r => r.Employees)
            .FirstOrDefault(x => x.Id == subjectSessionRewardId);

        if (subjectSessionReward == null)
            return null;

        return new SubjectSessionReward(subjectSessionReward, dbContext);
    }

    public static async Task<SubjectSessionReward? > FindByRewardAndSubject(int SubjectId,  int SessionRewardId, RewardDbContext dbContext)
    {
        var subjectSessions =
            await (
            from subjectSessionReward in dbContext.SubjectSessionRewardEntity
            join subjectSemester in dbContext.SubjectSemester on subjectSessionReward.SemesterSubjectId equals subjectSemester.Id

            where
            subjectSessionReward.SessionRewardId == SessionRewardId &&
            subjectSemester.SubjectId == SubjectId

            select subjectSessionReward
            ).SingleOrDefaultAsync();

        return (subjectSessions == null) ? null:
         new SubjectSessionReward(subjectSessions, dbContext);
    }

    public void Delete()
    {
        _dbContext.SubjectSessionRewardEntity.Remove(Entity);
    }

    public bool AddEmployee(int employeeId, bool isMainEmployee = false)
    {
        if (Entity.Employees.Any(x => x.EmployeeSnapshotId == employeeId))
            return false;

        if (Entity.Employees.Count >= MaxNumberOfEmployees)
            return false;

        Entity.Employees.Add(new EmployeeSessionRewardEntity { EmployeeSnapshotId = employeeId, SubjectSessionRewardId = Id });

        if (isMainEmployee)
            MainEmployeeId = employeeId;

        PrepareForSave();
        return true;
    }

    public bool[] AddEmployees(IEnumerable<int> employeeIds)
    {
        var results = employeeIds.Select(id => AddEmployee(id)).ToArray();
        PrepareForSave();
        return results;
    }

    public bool RemoveEmployee(int employeeId)
    {
        var employee = Entity.Employees.FirstOrDefault(x => x.EmployeeSnapshotId == employeeId);
        if (employee == null)
            return false;

        Entity.Employees.Remove(employee);

        if (MainEmployeeId == employeeId)
            MainEmployeeId = null;

        PrepareForSave();
        return true;
    }

    public static IEnumerable<SubjectSessionReward> GetAllForSession(int SessionRewardId, RewardDbContext dbContext)
    {
        return
            dbContext.SubjectSessionRewardEntity
            .Include(x => x.Employees)
            .Where(x => x.SessionRewardId == SessionRewardId)
            .Select(r => new SubjectSessionReward(r, dbContext))
            .ToArray();
    }

    private void PrepareForSave()
    {
        switch (_dbContext.Entry(Entity).State)
        {
            case EntityState.Detached:
                _dbContext.SubjectSessionRewardEntity.Add(Entity);
                break;

            case EntityState.Modified:
                _dbContext.SubjectSessionRewardEntity.Update(this.Entity);
                break;
        }
    }
}