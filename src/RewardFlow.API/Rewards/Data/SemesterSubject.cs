using RewardFlow_API.Common.Interface;

namespace Reward_Flow_v2.Rewards.Data;

public class SemesterSubject : ITenantEntity
{
    public int Id { get; set; }
    public Guid TenantId { get; set; }
    public int SubjectId { get; set; }
    public byte SemesterNumber { get; set; }
    public int NumberOfStudents { get; set; }
    public float? Price {  get; set; }
    
    public virtual Subject Subject { get; set; } = null!;
}