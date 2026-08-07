using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Reward_Flow_v2.User.Data.Database;
using RewardFlow_API.User.AuthService;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Reward_Flow_v2.User.AuthService;

public static class RefreshToken
{
    public record Request(string RefreshToken);
    public record Response(string JWTToken, string RefreshToken);

    public static void MapRefreshToken(this IEndpointRouteBuilder builder)
    {
        builder.MapPost(AuthApiPath.RefreshToken, HandlerAsync)
            .Produces<Response>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithTags(AuthApiPath.Tag);
    }

    private static async Task<IResult> HandlerAsync(Request request, TokenService tokenService, UserDbContext _dbContext, IConfiguration configuration, CancellationToken cancellationToken)
    {
        var user = await _dbContext.User
            .FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken && u.RefreshTokenExpiry > DateTime.UtcNow, cancellationToken);

        if (user == null)
            return Results.Unauthorized();
        
        var newJwtToken = tokenService.CreateToken(user);
        var newRefreshToken = tokenService.GenerateRefreshToken();

        user.UpdateRefreshToken(newRefreshToken);
        user.LastVisitedNow();
        
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(new Response(newJwtToken, newRefreshToken));
    }
}