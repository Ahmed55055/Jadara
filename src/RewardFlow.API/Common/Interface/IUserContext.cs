namespace RewardFlow_API.Common.Interface;

public interface IUserContext
{
    Guid Uuid { get; }
    Task<int> GetUserIdAsync();
    Guid GetTenantId();
    /// <summary>
    /// WARNING: Sets the tenant ID directly, bypassing the HTTP context.
    /// Use only in non-HTTP contexts such as background services or tests.
    /// </summary>
    /// <param name="tenantId">The tenant ID to set.</param>
    public void SetTenantId(Guid  tenantId);
}