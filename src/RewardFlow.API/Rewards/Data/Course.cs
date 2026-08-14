using Reward_Flow_v2.Rewards.Data;
using RewardFlow_API.Common.Interface;

namespace RewardFlow_API.Rewards.Data;

public sealed class Course : ITenantEntity
{
    public int Id { get; private set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = null!;
    public string? Code { get; set; }
    public bool IsTheoretical { get; set; }
    public bool IsPractical { get; set; }
    public decimal SubjectPrice { get; set; }
    
    public ICollection<TermCourse> TermCourse { get; set; } = new List<TermCourse>();
}