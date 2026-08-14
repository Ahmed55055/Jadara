namespace Reward_Flow_v2.Rewards.SessionsReward.EndPoints.Employees;

public record EmployeeSessionsDto(SessionsEmployee[] Employees, int RewardId, int SessionCount, decimal TotalAmount, CoursesAssignedDto[] CoursesAssigned);

public record CoursesAssignedDto(
    int CourseAssignmentId,
    int CourseId,
    string CourseName,
    int SessionCount
);

public record SessionsEmployee(
    int EmployeeId,
    string Name,
    decimal Salary
);