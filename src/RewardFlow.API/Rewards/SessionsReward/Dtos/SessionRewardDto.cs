namespace Reward_Flow_v2.Rewards.SessionsReward.Dtos;

public record SessionRewardDto(
    int SessionRewardId,
    string Name,
    string? Code,
    short? Year,
    byte? Semester,
    decimal Percentage,
    decimal Total,
    int CreatedBy
);