using Microsoft.AspNetCore.Routing;
using Reward_Flow_v2.User;
using Reward_Flow_v2.User.Data.Database;

namespace RewardFlow.IntegrationTests.Auth.Common;

public static class RequestResetPassword
{
    public record Request(string email);
    
    public static void MapUserLogin(this IEndpointRouteBuilder app)
    {
        app.MapPost(AuthApiPath.RequestResetPassword, HandlerAsync);
    }

    private static async Task<IResult> HandlerAsync(Request request, UserDbContext userDbContext,
        IResetPasswordMessageSender resetPasswordMessageSender)
    {
        throw new NotImplementedException();
    }
}