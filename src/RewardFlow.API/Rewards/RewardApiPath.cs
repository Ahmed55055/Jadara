namespace Reward_Flow_v2.Rewards;

public static class RewardApiPath
{
    public const string Tag = "rewards";
    private const string RewardRootApi = $"{ApiPath.Route}/{Tag}";
    public const string SessionsRootApi = $"{RewardRootApi}/sessions";
    
    public const string CreateSessionsReward = $"{SessionsRootApi}";
    public const string GetAllSessionsRewards = $"{SessionsRootApi}";
    public const string GetSessionsRewardById = $"{SessionsRootApi}/{{id}}";
    public const string UpdateSessionsReward = $"{SessionsRootApi}/{{id}}";
    public const string DeleteSessionsReward = $"{SessionsRootApi}/{{id}}";
    public const string GetSessionsRewardsByRewardId = $"{SessionsRootApi}/reward/{{rewardId}}";
    public const string AssignSessionRewardEmployees = $"{SessionsRootApi}/{{id}}/assign";
    public const string AddMultipleEmployeeSessions = $"{SessionsRootApi}/{{id}}/employees/batch";
}