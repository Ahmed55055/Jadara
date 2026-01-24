namespace RewardFlow.IntegrationTests.Auth.Common;

public interface IResetPasswordMessageSender
{
    Task SendToken(string email, string token);
}