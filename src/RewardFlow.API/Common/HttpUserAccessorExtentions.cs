using Microsoft.AspNetCore.Http;
using Reward_Flow_v2.Common.UserIdRetrieval;
using System.Security.Claims;

namespace Reward_Flow_v2.Common;

public static class HttpUserAccessorExtentions
{
    public static string? GetCurrentUserId(this IHttpContextAccessor httpContextAccessor)
    {
        return
            httpContextAccessor
                .HttpContext?
                .User
                .Claims
                .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?
                .Value;
    }

    public static Guid GetCurrentUserGuidId(this IHttpContextAccessor httpContextAccessor)
    {
        var userId = GetCurrentUserId(httpContextAccessor);

        if (userId is null)
            return Guid.Empty;

        Guid GuidId;

        if (!Guid.TryParse(userId, out GuidId))
            return Guid.Empty;

        return GuidId;
    }

    public static async Task<int> GetCurrentUserIntIdAsync(this IHttpContextAccessor httpContextAccessor,
        CancellationToken cancellationToken = default)
    {
        var userGuidId = GetCurrentUserGuidId(httpContextAccessor);
        if (userGuidId == Guid.Empty) return 0;

        var userIdService = httpContextAccessor.HttpContext?.RequestServices.GetService<IUserRetrievalService>();
        return userIdService != null ? await userIdService.GetUserIntIdAsync(userGuidId, cancellationToken) : 0;
    }

    public static async Task<int> GetCurrentUserIntIdAsync(this HttpContext httpContext,
        CancellationToken cancellationToken = default)
    {
        var userGuidId = ParseUserUuid(httpContext);
        if (userGuidId == Guid.Empty) return 0;

        var userRetrievalService = httpContext?.RequestServices.GetService<IUserRetrievalService>();
        return userRetrievalService != null ? await userRetrievalService.GetUserIntIdAsync(userGuidId, cancellationToken) : 0;
    }

    public static async Task<ScopedUserContextDto?> GetCurrentUserAsync(this HttpContext httpContext)
    {
        var uuid = ParseUserUuid(httpContext);
        var userRetrievalService = httpContext?.RequestServices.GetService<IUserRetrievalService>();
        return await userRetrievalService?.GetUserAsync(uuid);
    }

    public static Guid ParseUserUuid(this HttpContext httpContext)
    {
        var nameIdentifier = httpContext?
            .User
            .Claims
            .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?
            .Value;

        if (nameIdentifier is null || !Guid.TryParse(nameIdentifier, out Guid uuid))
            return Guid.Empty;

        return uuid;
    }
}