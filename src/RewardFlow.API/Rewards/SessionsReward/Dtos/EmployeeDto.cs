namespace Reward_Flow_v2.Rewards.SessionsReward.Dtos;

// TODO: This dto should be cleaned up and deleted
public record EmployeeDto
{
    public required int EmployeeId { get; init; }
    public decimal? Salary { get; init; }
}
