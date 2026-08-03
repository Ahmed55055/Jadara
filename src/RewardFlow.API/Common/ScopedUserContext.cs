namespace Reward_Flow_v2.Common;

public record ScopedUserContextDto(int Id, Guid Uuid, string Username, bool IsActive, int RoleId, int PlanId);

public class ScopedUserContext(IHttpContextAccessor httpContextAccessor)
{
    
    // In-memory cache for the duration of ONE request
    private ScopedUserContextDto? _cachedUser = null;
    public ScopedUserContextDto? User => _cachedUser;

    public async Task<ScopedUserContextDto?> GetFullUserAsync()
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