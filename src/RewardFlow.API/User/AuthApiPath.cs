namespace Reward_Flow_v2.User;

public static class AuthApiPath
{
    public const string Tag = "Auth";
    private const string AuthRootApi = $"{ApiPath.Route}/{Tag}";

    public const string Register = $"{AuthRootApi}/Register";
    public const string Login = $"{AuthRootApi}/login";
    public const string RefreshToken = $"{AuthRootApi}/refresh-token";
    public const string RequestResetPassword = $"{AuthRootApi}/request-reset-password";
    public const string VerifyResetToken = $"{AuthRootApi}/verify-reset-token";
    public const string ConfirmResetPassword = $"{AuthRootApi}/confirm-reset-password";

    private const string UserRootApi = $"{ApiPath.Route}/Users";
    public const string GetUserByUsername = $"{UserRootApi}/{{username}}";
    public const string GetUserByEmail = $"{UserRootApi}/email/{{email}}";
    public const string GetAllUsers = $"{UserRootApi}";
}
