using Reward_Flow_v2.Rewards.Data;

namespace RewardFlow_API.Rewards.Data;

public sealed class SubjectSnapshot
{
    public Guid SnapshotId { get; private set; }
    public DateTime CapturedAt { get; private set; }
    public int SemesterSubjectId { get; set; }
    public string SubjectName { get; set; } = null!;
    public bool IsTheoretical { get; set; }
    public bool IsPractical { get; set; }
    public byte Semester { get; set; }
    public byte Year { get; set; }

    public SemesterSubject SemesterSubject { get; set; } = null!;
}