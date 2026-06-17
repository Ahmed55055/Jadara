using RewardFlow_API.Common.Interface;

namespace Reward_Flow_v2.Rewards.Data;

public class EmployeeReward: ITenantEntity
{
    public int Id { get; set; }
    public Guid TenantId { get; set; }
    public int RewardId { get; set; }
    public int EmployeeId { get; set; }
    public float Total { get; set; }
    public bool IsUpdated { get; set; }
}