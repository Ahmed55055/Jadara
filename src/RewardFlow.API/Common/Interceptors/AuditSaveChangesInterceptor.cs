using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RewardFlow_API.Common.Interface;

namespace Reward_Flow_v2.Common.Interceptors;

public class AuditSaveChangesInterceptor(IUserContext scopedUserContextDto) : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = new CancellationToken())
    {
        ApplyAudit(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ApplyAudit(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    private async Task ApplyAudit(DbContext? context)
    {
        if (context is null) return;
        
        var currentUserId = await scopedUserContextDto.GetUserIdAsync();

        var addedEntities = context.ChangeTracker.Entries<IUserCreatable>()
            .Where(e => e.State == EntityState.Added);

        foreach (var entityEntry in addedEntities)
        {
            entityEntry.Entity.CreatedBy = currentUserId;
        }
        
    }
}