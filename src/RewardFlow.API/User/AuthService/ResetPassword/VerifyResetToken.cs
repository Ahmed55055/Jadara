namespace RewardFlow.IntegrationTests.Auth.Common;

public static class VerifyResetToken
{
    public record Request(string Token);
    
    public static void MapUserLogin(this IEndpointRouteBuilder builder)
    {
        throw new NotImplementedException();
    }
}