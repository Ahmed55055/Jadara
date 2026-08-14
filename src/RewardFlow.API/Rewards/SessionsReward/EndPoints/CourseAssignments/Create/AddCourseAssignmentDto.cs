namespace RewardFlow_API.Rewards.SessionsReward.EndPoints.CourseAssignments.Create;

/// <summary>
/// Employee infor needed for the assignment
/// </summary>
/// <param name="EmployeeId">employee id in the database</param>
/// <param name="Salary">optional salary. if set it uses the value,
/// and if null uses the current value with in the employee's data</param>
public record AssignEmployeeDto(int EmployeeId, decimal? Salary);
public record AddCourseAssignmentDto
{
    public required int RewardId { get; init; }
    public required int CourseId { get; init; }
    /// <summary>
    /// The number of enrolled students for this specific course assignment.
    /// </summary>
    /// <remarks>
    /// This value is an independent snapshot and does not affect the parent TermCourse.
    /// Resolution logic:
    /// <para>- If an integer is provided, it is persisted exclusively to this CourseAssignment.</para>
    /// - If null is provided, the system falls back to the parent TermCourse's current enrolled student count at the time of creation.
    /// </remarks>
    public int? StudentCount { get; init; }
    public required int MainEmployeeId { get; init; }
    public required IEnumerable<AssignEmployeeDto> Employees { get; init; }
}