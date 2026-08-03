namespace Reward_Flow_v2.Common.UserIdRetrieval;

public interface IUserRetrievalService
{
    Task<int> GetUserIntIdAsync(Guid userGuid, CancellationToken cancellationToken = default);
    Task<ScopedUserContextDto?> GetUserAsync(Guid userGuid, CancellationToken cancellationToken = default);
}