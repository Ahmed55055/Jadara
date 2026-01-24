namespace RewardFlow.IntegrationTests.Auth.Common;

public static class VerifyResetToken
{
    public record Request(string Token);
    public record Response(bool IsValid, string Message);
    
    public static void MapUserLogin(this IEndpointRouteBuilder builder)
    {
        throw new NotImplementedException();
    }
}