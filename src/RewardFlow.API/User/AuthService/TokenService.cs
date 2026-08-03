using Microsoft.IdentityModel.Tokens;
using Reward_Flow_v2.User.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RewardFlow_API.User.AuthService;

public static class TokenService
{
    public static string CreateToken(Reward_Flow_v2.User.Data.User user, IConfiguration configuration)
    {
        var Claims = new Claim[]
        {
            new Claim (ClaimTypes.NameIdentifier, user.UUID.ToString()),
            new Claim (ClaimTypes.Name, user.Username),
            new Claim (ClaimTypes.Role, Enum.GetName(typeof( UserRoleEnum),user.RoleId)!),
            new Claim("TenantId", user.TenantId.ToString())
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration.GetValue<string>("JWT:Token")!));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

        var tokenDescripter = new JwtSecurityToken(
            issuer: configuration.GetValue<string>("JWT:Issuer"),
            audience: configuration.GetValue<string>("JWT:Audience"),
            claims: Claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(tokenDescripter);
    }

    public static string GenerateRefreshToken()
    {
        var randomBytes = new byte[32];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}