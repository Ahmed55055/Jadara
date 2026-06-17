using RewardFlow_API.Common.Interface;

namespace Reward_Flow_v2.Employees.Data;

public class EmployeeNameToken : ITenantEntity
{
    public int Id { get; set; }
    public Guid TenantId { get; set; }
    public int UserId { get; set; }
    public string TokenHashed { get; set; } = null!;
    public byte N { get; set; }
    public int EmployeeId { get; set; }
}