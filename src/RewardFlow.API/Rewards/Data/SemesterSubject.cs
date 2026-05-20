namespace Reward_Flow_v2.Rewards.Data;

public class SemesterSubject
{
    public int Id { get; set; }
    public int SubjectId { get; set; }
    public byte Semester { get; set; }
    public int NumberOfStudents { get; set; }
    public decimal? Price {  get; set; }
    public byte Year { get; set; }
    
    public virtual Subject Subject { get; set; } = null!;
}