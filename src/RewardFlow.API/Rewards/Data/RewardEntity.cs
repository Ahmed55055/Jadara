using RewardFlow_API.Common.Interface;

namespace Reward_Flow_v2.Rewards.Data;

public class RewardEntity: ITenantEntity
{
    public int Id { get; set; }
    public Guid TenantId { get; set; }
    public string? Name { get; set; }
    public float Total { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdate { get; set; }
    public int CreatedBy { get; set; }
    public string? Code { get; set; }
    public int RewardType { get; set; }
    public int NumberOfEmployees { get; set; }


    public virtual ICollection<EmployeeReward> EmployeeRewards { get; set; } = new List<EmployeeReward>();
}