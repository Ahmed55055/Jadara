using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RewardFlow_API.Common.Interface;

namespace Reward_Flow_v2.Common.Interceptors;

public class TenantSaveChangesInterceptor(IUserContext userContext) : SaveChangesInterceptor
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

    private void ApplyAudit(DbContext? context)
    {
        if (context is null) return;

        var addedEntities = context.ChangeTracker.Entries<ITenantEntity>()
            .Where(e => e.State == EntityState.Added);

        foreach (var entityEntry in addedEntities.Where(e=>e.Entity.TenantId == Guid.Empty))
        {
            var currentTenantId = userContext.GetTenantId();
            entityEntry.Entity.TenantId = currentTenantId;
        }
        
    }
    
}