namespace Reward_Flow_v2.Rewards.Data;

public sealed class EmployeeReward
{
    public int RewardId { get; init; }
    public int EmployeeId { get; init; }
    public Guid EmployeeSnapshotId { get; private set; }
    public decimal Amount { get; private set; }
    public bool IsUpdated { get; private set; }

    public EmployeeSnapshot EmployeeSnapshot { get; private set; } 

    private EmployeeReward() { }

    private EmployeeReward(int rewardId, int employeeId, EmployeeSnapshot employeeSnapshot, decimal amount)
    {
        RewardId = rewardId;
        EmployeeId = employeeId;
        Amount = amount;
        IsUpdated = true;
        EmployeeSnapshot = employeeSnapshot;
    }

    public static EmployeeReward Create(int rewardId, EmployeeSnapshot employeeSnapshot, decimal total = 0)
    {
        ArgumentNullException.ThrowIfNull(employeeSnapshot);

        var instance = new EmployeeReward(rewardId, employeeSnapshot.EmployeeId, employeeSnapshot, total);

        if (total == 0)
            instance.MarkAsOutdated();

        return instance;
    }

    public void MarkAsUpdated()
    {
        IsUpdated = true;
    }

    public void MarkAsOutdated()
    {
        IsUpdated = false;
    }

    public void UpdateAmount(decimal total)
    {
        Amount = total;
        IsUpdated = true;
    }
}