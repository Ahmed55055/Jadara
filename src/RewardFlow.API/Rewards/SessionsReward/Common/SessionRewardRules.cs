namespace Reward_Flow_v2.Rewards.SessionsReward.Common;

public class SessionRewardRules : ISessionRewardRules
{
    // TODO: Move this to database configuration in the future
    private const int MAX_SESSIONS_LIMIT = 250;

    public SessionRewardRules()
    {
        MaxSessionsNumber = MAX_SESSIONS_LIMIT;
        MaxSessionNumberRuleEnabled = true;
    }

    public int MaxSessionsNumber { get; set; }
    public bool MaxSessionNumberRuleEnabled { get; set; }

    public bool IsWithInMaximumNumberOfSession(int sessionNumber)
    {
        return !MaxSessionNumberRuleEnabled || sessionNumber <= MaxSessionsNumber;
    }

    public bool CanAssignEmployeeToSubjectAsync(int SubjectId, int NumberOfStudents,
        IEnumerable<int> EmployeesIds)
    {
        return EmployeesIds.All(e => e > 0) &&
               SubjectId > 0 &&
               NumberOfStudents > 0;
    }

    public int GetAllowedSessionCount(int sessionCount)
    {
        return IsWithInMaximumNumberOfSession(sessionCount) && MaxSessionNumberRuleEnabled ? sessionCount : MaxSessionsNumber;
    }
}
