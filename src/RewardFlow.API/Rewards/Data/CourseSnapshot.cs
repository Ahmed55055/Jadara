using Reward_Flow_v2.Rewards.Data;

namespace RewardFlow_API.Rewards.Data;

public sealed class CourseSnapshot
{
    public Guid SnapshotId { get; private set; }
    public DateTime CapturedAt { get; private set; }
    public int CourseId { get; init; }
    public int TermCourseId { get; set; }
    public string CourseName { get; set; } = null!;
    public int? StudentCount { get; set; }
    public bool IsTheoretical { get; set; }
    public bool IsPractical { get; set; }
    public byte Term { get; set; }
    public short Year { get; set; }

    public TermCourse TermCourse { get; set; } = null!;
}