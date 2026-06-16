using Microsoft.EntityFrameworkCore;
using Reward_Flow_v2.User.Data.Database;

namespace RewardFlow_API.Common.Interface;

public class UserContext(IHttpContextAccessor httpContextAccessor, UserDbContext dbContext) : IUserContext
{
    private const string TenantIdHeaderName = "TenantId";
    public Guid Uuid
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    public int GetUserId()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Gets the tenant ID from the current HTTP request
    /// </summary>
    /// <returns>Tenant Id</returns>
    /// <exception cref="ApplicationException">Thrown when the tenant ID header is not present</exception>
    public Guid GetTenantId()
    {
        var tenantIdHeader = httpContextAccessor.HttpContext?
            .Request
            .Headers[TenantIdHeaderName];

        if (!tenantIdHeader.HasValue ||
            !Guid.TryParse(tenantIdHeader, out var tenantId))
        {
            throw new ApplicationException($"Required header '{TenantIdHeaderName}' is missing.");
        }
        
        return tenantId;
    }
}