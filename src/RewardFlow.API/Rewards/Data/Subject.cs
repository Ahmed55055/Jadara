using RewardFlow_API.Common.Interface;

namespace Reward_Flow_v2.Rewards.Data;

public class Subject : ITenantEntity
{
    public int Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = null!;
    public bool IsTheoretical { get; set; }
    public bool IsPractical { get; set; }
    public float SubjectPrice { get; set; }
    
    public virtual ICollection<SemesterSubject> SubjectSemesters { get; set; } = new List<SemesterSubject>();
}