namespace Reward_Flow_v2.Common;

public record UserContext(int Id, Guid Uuid, string Username, bool IsActive, int RoleId, int PlanId);

public class ScopedUserContext(IHttpContextAccessor httpContextAccessor)
{
    
    // In-memory cache for the duration of ONE request
    private UserContext? _cachedUser = null;
    public UserContext? User => _cachedUser;

    public async Task<UserContext?> GetFullUserAsync()
    {
        var  httpContext = httpContextAccessor.HttpContext;
        
        if (_cachedUser is not null) 
            return _cachedUser;

        var requestUuid = httpContext.ParseUserUuid();

        if (requestUuid == Guid.Empty)
            return null;

        _cachedUser = await httpContext.GetCurrentUserAsync();
        return _cachedUser;
    }
}