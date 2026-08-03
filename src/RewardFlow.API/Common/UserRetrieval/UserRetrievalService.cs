using Microsoft.EntityFrameworkCore;
using Reward_Flow_v2.User.Data.Database;

namespace Reward_Flow_v2.Common.UserIdRetrieval;

public class UserRetrievalService : IUserRetrievalService
{
    private readonly UserDbContext _dbContext;

    public UserRetrievalService(UserDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> GetUserIntIdAsync(Guid userGuid, CancellationToken cancellationToken = default)
    {
        return await _dbContext.User
            .Where(u => u.UUID == userGuid)
            .Select(u => u.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ScopedUserContextDto?> GetUserAsync(Guid userGuid, CancellationToken cancellationToken = default)
    {
        return await _dbContext.User
            .Where(u => u.UUID == userGuid)
            .Select(u => new ScopedUserContextDto(u.Id, u.UUID, u.Username, u.IsActive, u.RoleId, u.PlanId))
            .FirstOrDefaultAsync();
    }
}