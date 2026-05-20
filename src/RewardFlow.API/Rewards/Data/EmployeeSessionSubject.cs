using FluentResults;
using Reward_Flow_v2.Rewards.Data.Database;
using System.Threading.Tasks;

namespace Reward_Flow_v2.Rewards.Data;

public class EmployeeSessionSubject
{
    public int SubjectSessionRewardId { get; init; }
    public Guid EmployeeSnapshotId { get; init; }
    // Denormalization to optimize queries by avoiding joins.
    // Keep set private; this value must remain immutable and never be updated independently.
    public int EmployeeId { get; private set; }
    
    public virtual SubjectSessionRewardEntity SubjectSessionReward { get; init; } = null!;
    public virtual EmployeeSnapshot EmployeeSnapshot { get; init; } = null!;
}
