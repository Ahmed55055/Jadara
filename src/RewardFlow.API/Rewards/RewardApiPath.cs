namespace Reward_Flow_v2.Rewards;

public static class RewardApiPath
{
    // ============= ROOT =============
    public const string Tag = "rewards";
    private const string RewardRootApi = $"{ApiPath.Route}/{Tag}";
    
    // ============= SESSION REWARD =============
    public const string SessionRewards = $"{RewardRootApi}/sessions";
    public const string SessionRewardsById = $"{SessionRewards}/{{rewardId}}";
    
    // --- Business Data ---
    
    // Course Assignment
    public const string CourseAssignments = $"{SessionRewardsById}/course-assignments";
    public const string CourseAssignmentsById = $"{SessionRewardsById}/course-assignments/{{courseAssignmentId}}";
    
    // Employees Sessions
    public const string EmployeeSessions = $"{SessionRewardsById}/employees";
    public const string EmployeeSessionsById = $"{SessionRewardsById}/employees/{{employeeId}}";
}