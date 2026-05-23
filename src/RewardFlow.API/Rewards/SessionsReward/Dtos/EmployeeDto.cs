namespace Reward_Flow_v2.Rewards.SessionsReward.Dtos;

public record EmployeeDto
{
    public required int EmployeeId { get; init; }
    public required decimal Salary { get; init; }
}
