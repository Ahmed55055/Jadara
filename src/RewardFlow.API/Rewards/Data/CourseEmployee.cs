using FluentResults;
using Reward_Flow_v2.Rewards.Data.Database;
using System.Threading.Tasks;

namespace Reward_Flow_v2.Rewards.Data;

public sealed class CourseEmployee
{
    public int SubjectSessionRewardId { get; init; }
    public int EmployeeId { get; private set; }
    public Guid EmployeeSnapshotId { get; init; }
    
    public CourseAssignment Course { get; init; } = null!;
    public EmployeeSnapshot EmployeeSnapshot { get; init; } = null!;
    private CourseEmployee() { } 

    public CourseEmployee(CourseAssignment course, EmployeeSnapshot employeeSnapshot)
    {
        ArgumentNullException.ThrowIfNull(course, nameof(course));
        ArgumentNullException.ThrowIfNull(employeeSnapshot, nameof(employeeSnapshot));
        
        Course = course;
        EmployeeSnapshot = employeeSnapshot;
        
        EmployeeId = employeeSnapshot.EmployeeId; 
    }
}
