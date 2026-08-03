using Microsoft.EntityFrameworkCore;
using Reward_Flow_v2.Common;
using Reward_Flow_v2.User.Data.Database;

namespace RewardFlow_API.Common.Interface;

public class UserContext(IHttpContextAccessor httpContextAccessor, UserDbContext dbContext) : IUserContext
{
    private const string TenantIdHeaderName = "TenantId";
    private Guid? _tenantId;

    public Guid Uuid
    {
        get
        {
            return httpContextAccessor.GetCurrentUserGuidId();
        }
    }

    public async Task<int> GetUserIdAsync()
    {
        return await httpContextAccessor.GetCurrentUserIntIdAsync();
    }

    /// <summary>
    /// Gets the tenant ID from the current HTTP request
    /// </summary>
    /// <returns>Tenant Id</returns>
    /// <exception cref="ApplicationException">Thrown when the tenant ID header is not present</exception>
    public Guid GetTenantId()
    {
        if(_tenantId is not null)
            return _tenantId.Value;
        
        var tenantIdClaim = httpContextAccessor.HttpContext?
            .User
            .FindFirst(TenantIdHeaderName)
            .Value;

        if (tenantIdClaim is null || !Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            throw new ApplicationException($"Required header '{TenantIdHeaderName}' is missing.");
        }

        _tenantId = tenantId;
        return tenantId;
    }

    /// <summary>
    /// WARNING: Sets the tenant ID directly, bypassing the HTTP context.
    /// Use only in non-HTTP contexts such as background services or tests.
    /// </summary>
    /// <param name="tenantId">The tenant ID to set.</param>
    public void SetTenantId(Guid tenantId) =>  _tenantId = tenantId;

}