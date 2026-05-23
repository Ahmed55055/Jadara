namespace Reward_Flow_v2.Rewards.Data;

public sealed class SemesterSubject
{
    public int Id { get; private set; }
    public int SubjectId { get; init; }
    public byte Semester { get; init; }
    public int NumberOfStudents { get; set; }
    public decimal? Price {  get; set; }
    public byte Year { get; init; }
    
    public Subject Subject { get; init; } = null!;
}