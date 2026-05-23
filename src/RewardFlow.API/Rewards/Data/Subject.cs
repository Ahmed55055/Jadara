namespace Reward_Flow_v2.Rewards.Data;

public sealed class Subject
{
    public int Id { get; private set; }
    public string Name { get; set; } = null!;
    public bool IsTheoretical { get; set; }
    public bool IsPractical { get; set; }
    public decimal SubjectPrice { get; set; }
    
    public ICollection<SemesterSubject> SubjectSemesters { get; set; } = new List<SemesterSubject>();
}