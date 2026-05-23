using FluentResults;
using Reward_Flow_v2.Rewards.Data.Database;
using System.Threading.Tasks;

namespace Reward_Flow_v2.Rewards.Data;

public sealed class EmployeeSessionSubject
{
    public int SubjectSessionRewardId { get; init; }
    public int EmployeeId { get; private set; }
    public Guid EmployeeSnapshotId { get; init; }
    
    public SubjectSessionRewardEntity SubjectSessionReward { get; init; } = null!;
    public EmployeeSnapshot EmployeeSnapshot { get; init; } = null!;
    private EmployeeSessionSubject() { } 

    public EmployeeSessionSubject(SubjectSessionRewardEntity subjectSessionReward, EmployeeSnapshot employeeSnapshot)
    {
        ArgumentNullException.ThrowIfNull(subjectSessionReward, nameof(subjectSessionReward));
        ArgumentNullException.ThrowIfNull(employeeSnapshot, nameof(employeeSnapshot));
        
        SubjectSessionReward = subjectSessionReward;
        EmployeeSnapshot = employeeSnapshot;
        
        EmployeeId = employeeSnapshot.EmployeeId; 
    }
}
