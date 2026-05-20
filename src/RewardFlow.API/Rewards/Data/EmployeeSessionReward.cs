using Reward_Flow_v2.Rewards.Data;

namespace RewardFlow_API.Rewards.Data;

public class EmployeeSessionReward
{
    public int SessionRewardId { get; init; }
    public int EmployeeId { get; private set; }
    public Guid EmployeeSnapshotId { get; init; }
    public int SessionsCount { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public virtual EmployeeSnapshot EmployeeSnapshot { get; init; }

    private EmployeeSessionReward(int sessionRewardId, EmployeeSnapshot employeeSnapshot)
    {
        SessionRewardId = sessionRewardId;
        EmployeeSnapshot = employeeSnapshot;
        EmployeeId = employeeSnapshot.EmployeeId;
        UpdatedAt = DateTime.UtcNow;
    }

    public static EmployeeSessionReward? Create(int sessionRewardId, EmployeeSnapshot employeeSnapshot)
    {
        if(employeeSnapshot is null)
            return null;
        
        return new EmployeeSessionReward(sessionRewardId, employeeSnapshot);
    }

    public void UpdateSessionCount(int sessionsCount)
    {
        SessionsCount = sessionsCount;
        UpdatedAt = DateTime.UtcNow;
    }
}