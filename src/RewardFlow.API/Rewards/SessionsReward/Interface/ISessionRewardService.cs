using FluentResults;
using Reward_Flow_v2.Rewards.Data;
using Reward_Flow_v2.Rewards.SessionsReward.Dtos;
using RewardFlow_API.Rewards.Data;
using RewardFlow_API.Rewards.SessionsReward.EndPoints.CourseAssignments.Create;
using RewardFlow_API.Rewards.SessionsReward.EndPoints.RewardManage.CreateReward;

namespace Reward_Flow_v2.Rewards.SessionsReward;

public record EmployeeRewardDto(int EmployeeRewardId,int EmployeeId,decimal Total);

internal interface ISessionRewardService : IReward
{
    public Task<Result<int>> CreateReward(CreateSessionsReward.Request dto, int createdBy);
    /// <summary>
    /// Orchestrates the assignment of employees to a specific course term, enforcing capacity and eligibility rules.
    /// </summary>
    /// <param name="dto">The data transfer object containing employee identifiers and target course details.</param>
    /// <returns>A success result containing the assigned course entity, or a failure result if validation constraints are violated or an internal error occurs.</returns>
    public Task<Result<CourseAssignment?>> AssignEmployeeAsync(AddCourseAssignmentDto dto);
    //public Task<Result> RemoveCourseAssignmnetAsync(int courseId);
    //public Task<Result<CourseAssignment?>> GetCourseAssignmentAsync(int courseAssignmentId);
    public Task<Result<EmployeeRewardDto?>> GetEmployeeReward(int employeeId);
    public Task<IEnumerable<EmployeeRewardDto>> GetEmployeesRewards();
}