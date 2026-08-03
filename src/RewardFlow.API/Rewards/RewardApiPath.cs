namespace Reward_Flow_v2.Rewards;

public static class RewardApiPath
{
    // ============= ROOT =============
    public const string Tag = "rewards";
    private const string RewardRootApi = $"{ApiPath.Route}/{Tag}";
    
    // ============= SESSION REWARD =============
    public const string SessionRewards = $"{RewardRootApi}/sessions";
    public const string SessionRewardsById = $"{SessionRewards}/{{id}}";
    
    // --- Business Data ---
    
    // Course Assignment
    public const string CourseAssignments = $"{SessionRewardsById}/course-assignments";
    public const string CourseAssignmentsById = $"{SessionRewardsById}/course-assignments/{{course-assignment-id}}";
    
}