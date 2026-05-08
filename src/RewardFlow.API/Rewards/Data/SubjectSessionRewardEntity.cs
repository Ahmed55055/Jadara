using FluentResults;
using Reward_Flow_v2.Rewards.Data.Database;
using System.Threading.Tasks;

namespace Reward_Flow_v2.Rewards.Data;

public class SubjectSessionRewardEntity
{
    public int Id { get; set; }
    public int SessionRewardId { get; set; }
    public int NumberOfSessions { get; set; }
    public int SemesterSubjectId { get; set; }
    public int StudentsNumber { get; set; }
    public int? MainEmployeeId { get; set; }
    public int MaxNumberOfEmployees {  get; set; }

    public virtual ICollection<EmployeeSessionRewardEntity> Employees { get; set; } = new List<EmployeeSessionRewardEntity>();
    public virtual  ICollection<SemesterSubject> SemesterSubject { get; set; } = new List<SemesterSubject>();
}
