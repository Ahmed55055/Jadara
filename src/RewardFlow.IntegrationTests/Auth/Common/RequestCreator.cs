using Reward_Flow_v2.User.AuthService.Register;
using Reward_Flow_v2.User.AuthService.Login;
using Reward_Flow_v2.User.Data;

namespace RewardFlow.IntegrationTests.Auth.Common;

public static class RequestCreator
{
    /// <summary>
    /// Creates a Register.Request object from a User entity.
    /// This helper method maps user data to the request format expected by the register endpoint.
    /// </summary>
    /// <param name="user">The user entity containing the data to map</param>
    /// <returns>A Register.Request object ready for API submission</returns>
    public static Register.Request CreateRegisterRequest(User user, string password)
    {
        return new Register.Request(user.Username,password,user.Email);
    }

    /// <summary>
    /// Creates a Login.Request object from a User entity and password.
    /// This helper method maps user data to the request format expected by the login endpoint.
    /// </summary>
    /// <param name="user">The user entity containing the username</param>
    /// <param name="password">The plain text password to use</param>
    /// <returns>A Login.Request object ready for API submission</returns>
    public static Login.Request CreateLoginRequest(User user, string password)
    {
        return new Login.Request(user.Username, password);
    }

    /// <summary>
    /// Creates a RequestResetPassword.Request object from an email.
    /// </summary>
    /// <param name="email">The email address for password reset</param>
    /// <returns>A RequestResetPassword.Request object ready for API submission</returns>
    public static RequestResetPassword.Request CreateRequestResetPasswordRequest(string email)
    {
        return new RequestResetPassword.Request(email);
    }

    /// <summary>
    /// Creates a VerifyResetToken.Request object from a token.
    /// </summary>
    /// <param name="token">The reset token to verify</param>
    /// <returns>A VerifyResetToken.Request object ready for API submission</returns>
    public static VerifyResetToken.Request CreateVerifyResetTokenRequest(string token)
    {
        return new VerifyResetToken.Request(token);
    }

    /// <summary>
    /// Creates a ConfirmResetPassword.Request object from a token and new password.
    /// </summary>
    /// <param name="token">The reset token</param>
    /// <param name="newPassword">The new password</param>
    /// <returns>A ConfirmResetPassword.Request object ready for API submission</returns>
    public static ConfirmResetPassword.Request CreateConfirmResetPasswordRequest(string token, string newPassword)
    {
        return new ConfirmResetPassword.Request(token, newPassword);
    }
}