using FluentValidation;
using Reward_Flow_v2.Common.Enums;
using Reward_Flow_v2.User.Data;
using System.Security.Claims;

namespace Reward_Flow_v2.Common.EndpointValidation;

public static class ValidateEndpoint
{
    public static IEndpointConventionBuilder Validation<T>(this IEndpointConventionBuilder builder,
        AbstractValidator<T> validator)
    {
        builder.AddEndpointFilter(async (context, next) =>
        {
            var request = context.Arguments.OfType<T>().FirstOrDefault();

            if (request is null)
                return Results.BadRequest();

            var validationResult = validator.Validate(request);

            if (!validationResult.IsValid)
                return Results.BadRequest(validationResult.Errors);

            var result = await next(context);

            return result;
        });

        return builder;
    }
    
    /// <summary>
    /// Secures an endpoint by validating user authentication and roles, returning 401 Unauthorized if access is denied.
    /// </summary>
    /// <param name="builder">The endpoint builder to attach the filter to.</param>
    /// <param name="allowedRoles">The roles permitted to access the endpoint.</param>
    /// <param name="strategy">The validation strategy (JWT claims or Database lookup).</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <remarks>
    /// If <see cref="AllowedRoles.Admin"/> is included in <paramref name="allowedRoles"/>, 
    /// the strategy is automatically forced to <see cref="ValidationStrategy.Database"/> for security.
    /// </remarks>
    public static IEndpointConventionBuilder ValidateAccess(
        this IEndpointConventionBuilder builder,
        AllowedRoles allowedRoles = AllowedRoles.Owner,
        ValidationStrategy strategy = ValidationStrategy.Database)
    {
        return builder.AddEndpointFilter(async (context, next) =>
        {
            var httpContext = context.HttpContext;
            var user = httpContext.User;

            if (user?.Identity?.IsAuthenticated != true)
            {
                return Results.Unauthorized();
            }

            // admin should be database checked every time. even if it's slow access but better than system level vournability.
            if (allowedRoles.HasFlag(AllowedRoles.Admin))
                strategy = ValidationStrategy.Database;

            var isValid = strategy switch
            {
                ValidationStrategy.JwtOnly => ValidateJwtClaims(httpContext, allowedRoles),
                ValidationStrategy.Database => await ValidateDatabasePermissions(httpContext, allowedRoles),
                _ => false
            };
            
            if(!isValid)
                return Results.Unauthorized();

            return await next(context);
        });
    }

    private static bool ValidateJwtClaims(HttpContext httpContext, AllowedRoles allowedRoles)
    {
        var uuid = httpContext.ParseUserUuid();

        if (allowedRoles.HasFlag(AllowedRoles.Owner))
            return uuid != Guid.Empty;

        if (allowedRoles.HasFlag(AllowedRoles.Admin))
            return httpContext?.User?.IsInRole("Admin") ?? false;

        return true;
    }

    private static async Task<bool> ValidateDatabasePermissions(HttpContext httpContext, AllowedRoles allowedRoles)
    {
        var scopedUserContext = httpContext.RequestServices.GetRequiredService<ScopedUserContext>();
        var contextUser = await scopedUserContext.GetFullUserAsync();
        
        var requestUuid = httpContext.ParseUserUuid();

        if (contextUser is null)
            return false;

        if (contextUser?.Uuid is var uuid && uuid == Guid.Empty || uuid != requestUuid)
            return false;

        if (allowedRoles.HasFlag(AllowedRoles.Owner))
            return true;


        if (allowedRoles.HasFlag(AllowedRoles.Admin) && (contextUser.RoleId != (int)UserRoleEnum.Admin))
            return false;

        return true;
    }

}