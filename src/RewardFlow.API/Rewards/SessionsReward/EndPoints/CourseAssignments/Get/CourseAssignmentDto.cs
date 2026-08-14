namespace RewardFlow_API.Rewards.SessionsReward.EndPoints.CourseAssignments.Get;

public record AssignedEmployeesDto(int EmployeeId);

public record CourseAssignmentDto(
    int CourseAssignmentId,
    int RewardId,
    int TermCourseId,
    int SessionsCount,
    int? LeadStaffId,
    List<AssignedEmployeesDto> AssignedEmployees);