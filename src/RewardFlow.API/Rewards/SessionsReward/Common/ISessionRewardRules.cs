using Reward_Flow_v2.Rewards.SessionsReward.Dtos;

namespace Reward_Flow_v2.Rewards.SessionsReward.Common;

public interface ISessionRewardRules
{
    int MaxSessionsNumber { get; set; }
    bool MaxSessionNumberRuleEnabled {  get; set; }

    bool IsWithInMaximumNumberOfSession(int sessionNumber);
    Task<bool> CanAssignEmployeeToSubjectAsync(int dtoSubjectId, int dtoNumberOfStudents, IEnumerable<int> EmployeesIds);
}