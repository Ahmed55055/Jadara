using FluentResults;
using Reward_Flow_v2.Rewards.SessionsReward.CreateReward;
using Reward_Flow_v2.Rewards.SessionsReward.Dtos;
using RewardFlow_API.Rewards.Data;

namespace Reward_Flow_v2.Rewards.SessionsReward;

public record EmployeeRewardDto(int EmployeeRewardId,int EmployeeId,decimal Total);

internal interface ISessionRewardService : IReward
{
    public Task<Result<int>> CreateReward(CreateSessionsReward.Request dto, int createdBy);
    public Task<Result<IEnumerable<EmployeeSessionReward>>> AssignEmployeeAsync(SessionSubjectDto dto);
    public Task<Result<EmployeeRewardDto?>> GetEmployeeReward(int employeeId);
    public Task<IEnumerable<EmployeeRewardDto>> GetEmployeesRewards();
}