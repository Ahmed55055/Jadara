using RewardFlow_API.Common.Interface;

namespace Reward_Flow_v2.Rewards.Data;

public class EmployeeReward: ITenantEntity
{
    public int RewardId { get; set; }
    public int EmployeeId { get; set; }
    public Guid TenantId { get; set; }
    public Guid EmployeeSnapshotId { get; set; }
    public decimal Total { get; set; }
    public bool IsUpdated { get; private set; }
    
    public virtual EmployeeSnapshot EmployeeSnapshot { get; set; }

    private EmployeeReward() { }

    private EmployeeReward(int rewardId, int employeeId,  EmployeeSnapshot employeeSnapshot, decimal total)
    {
        RewardId = rewardId;
        EmployeeId = employeeId;
        Total = total;
        IsUpdated = true;
        this.EmployeeSnapshot =  employeeSnapshot; 
    }

    public static EmployeeReward? Create(int rewardId, int employeeId, EmployeeSnapshot employeeSnapshot, decimal total = 0)
    {
        if (employeeSnapshot is null)
            return null;
        
        var instance = new EmployeeReward(rewardId, employeeId, employeeSnapshot, total);
        
        if(total == 0)
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

    public void UpdateTotal(decimal total)
    {
        Total = total;
        IsUpdated = true;
    }
}