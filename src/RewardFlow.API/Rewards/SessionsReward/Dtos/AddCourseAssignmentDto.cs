namespace Reward_Flow_v2.Rewards.SessionsReward.Dtos;

public record AddCourseAssignmentDto
{
    public required int RewardId { get; init; }
    public required int TermCourseId { get; init; }
    /// <summary>
    /// The number of enrolled students for this specific course assignment.
    /// </summary>
    /// <remarks>
    /// This value is an independent snapshot and does not affect the parent TermCourse.
    /// Resolution logic:
    /// <para>- If an integer is provided, it is persisted exclusively to this CourseAssignment.</para>
    /// - If null is provided, the system falls back to the parent TermCourse's current enrolled student count at the time of creation.
    /// </remarks>
    public int? NumberOfStudents { get; init; }
    public required int MainEmployeeId { get; init; }
    public required IEnumerable<int> EmployeesIds { get; init; }
}