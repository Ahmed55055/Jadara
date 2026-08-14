using RewardFlow_API.Common.Interface;
using RewardFlow_API.Rewards.Data;

namespace Reward_Flow_v2.Rewards.Data;

public sealed class TermCourse: ITenantEntity
{
    public int Id { get; private set; }
    public Guid TenantId { get; set; }
    public int CourseId { get; init; }
    public byte Term { get; init; }
    public int? StudentCount { get; set; }
    public decimal? Price {  get; set; }
    public short Year { get; init; }
    
    public Course Course { get; init; } = null!;

    public static TermCourse Create(Course course, byte semester, short year)
    {
        return new TermCourse
        {
            Course = course,
            CourseId = course.Id,
            Term = semester,
            Year = year
        };
    }
}