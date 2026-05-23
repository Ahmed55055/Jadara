namespace Reward_Flow_v2.Rewards.Data;

public class Reward
{
    public int Id { get; private set; }
    public string? Name { get; set; }
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime LastUpdate { get; set; }
    public int CreatedBy { get; init; }
    public string? Code { get; set; }
    public int RewardType { get; init; }
    public int NumberOfEmployees { get; set; }


    public virtual ICollection<EmployeeReward> EmployeeRewards { get; set; } = new List<EmployeeReward>();
}