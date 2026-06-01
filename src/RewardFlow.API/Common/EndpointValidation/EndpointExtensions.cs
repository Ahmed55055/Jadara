using Reward_Flow_v2.Common.Enums;

namespace Reward_Flow_v2.Common.EndpointValidation;

public static class EndpointExtensions
{
    public static IEndpointConventionBuilder PreloadUser(this IEndpointConventionBuilder builder)
    {
        return builder.AddEndpointFilter(async (context, next) =>
        {
            var userContext = context.HttpContext.RequestServices.GetRequiredService<ScopedUserContext>();
        
            await userContext.GetFullUserAsync(); 

            return await next(context);
        });
    }
}