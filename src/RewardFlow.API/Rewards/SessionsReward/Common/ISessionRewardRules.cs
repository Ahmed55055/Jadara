using Reward_Flow_v2.Rewards.SessionsReward.Dtos;

namespace Reward_Flow_v2.Rewards.SessionsReward.Common;

public interface ISessionRewardRules
{
    int MaxSessionsNumber { get; set; }
    bool MaxSessionNumberRuleEnabled {  get; set; }

    bool IsWithInMaximumNumberOfSession(int sessionNumber);
    bool CanAssignEmployeeToSubjectAsync(int SubjectId, int NumberOfStudents, IEnumerable<int> EmployeesIds);
    int GetAllowedSessionCount(int sessionCount);
}