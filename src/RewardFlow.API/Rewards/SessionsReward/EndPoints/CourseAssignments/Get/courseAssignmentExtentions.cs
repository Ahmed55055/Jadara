using Reward_Flow_v2.Rewards.Data;

namespace RewardFlow_API.Rewards.SessionsReward.EndPoints.CourseAssignments.Get;

public static class courseAssignmentExtentions
{
    public static CourseAssignmentDto MapResponseDto(this CourseAssignment assignment)
    {
        var employees = assignment.StaffMembers
            .Select(e => new AssignedEmployeesDto(e.EmployeeId))
            .ToList();

        return new CourseAssignmentDto(
            assignment.Id,
            assignment.SessionRewardId,
            assignment.TermCourseId,
            assignment.SessionCount,
            assignment.MainEmployeeId,
            employees);
    }
}