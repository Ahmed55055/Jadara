using Reward_Flow_v2.Rewards.SessionsReward.Dtos;

namespace Reward_Flow_v2.Rewards.SessionsReward;

public record EmployeeRewardDto(int EmployeeRewardId,int EmployeeId,float Total);

internal interface ISessionReward : IReward
{
    public Task<bool> AssignEmployeeAsync(SessionSubjectDto dto);
    public Task<bool> AssignEmployeesAsync(IEnumerable<SessionSubjectDto> dto);
    public Task<EmployeeRewardDto?> GetEmployeeReward(int employeeId);
    public Task<IEnumerable<EmployeeRewardDto>> GetEmployeesRewards();
}