namespace Reward_Flow_v2.Common.Enums;

public enum ValidationStrategy
{
    /// <summary>
    /// Reads the JWT and validates the role directly from the token
    /// </summary>
    JwtOnly,

    /// <summary>
    /// Reads the JWT and validates the role by querying the database for the user's role, and it's existence.
    /// </summary>
    Database
}