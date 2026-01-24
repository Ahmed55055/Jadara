namespace RewardFlow.IntegrationTests.Auth.Common;

public static class ConfirmResetPassword
{
    public record Request(string Token, string NewPassword);
    public record  Response(bool IsSuccess, string? ErrorMessage );
    public static void MapUserLogin(this IEndpointRouteBuilder builder)
    {
        throw new NotImplementedException();
    }
}