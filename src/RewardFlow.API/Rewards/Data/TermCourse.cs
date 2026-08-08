using RewardFlow_API.Common.Interface;
using RewardFlow_API.Rewards.Data;

namespace Reward_Flow_v2.Rewards.Data;

public sealed class TermCourse: ITenantEntity
{
    public int Id { get; private set; }
    public Guid TenantId { get; set; }
    public int CourseId { get; init; }
    public byte Semester { get; init; }
    public int NumberOfStudents { get; set; }
    public decimal? Price {  get; set; }
    public byte Year { get; init; }
    
    public Course Course { get; init; } = null!;
}