
using FluentResults;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Reward_Flow_v2.User.Data;
using Reward_Flow_v2.User.Data.Database;
using RewardFlow_API.User.AuthService;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Reward_Flow_v2.User.AuthService.Login;

public static class Login
{
    public record Request(string username, string password);
    public record Response(string JWTToken, string RefreshToken);

    public static void MapUserLogin(this IEndpointRouteBuilder builder)
    {
        builder.MapPost(AuthApiPath.Login, HandlerAsync)
            .Produces<Login.Response>(StatusCodes.Status200OK)
            .Produces<IEnumerable<FluentValidation.Results.ValidationFailure>>(StatusCodes.Status400BadRequest)
            .WithTags(AuthApiPath.Tag);
    }

    private static async Task<IResult> HandlerAsync(Login.Request request, TokenService tokenService, UserDbContext _dbContext, IConfiguration configuration, CancellationToken cancellationToken)
    {
        var validationResult = new LoginUserRequestValidator().Validate(request);
        
        if (!validationResult.IsValid)
            return Results.BadRequest(validationResult.Errors);

        var user = await _dbContext.User            
            .FirstOrDefaultAsync(u => u.Username == request.username, cancellationToken);

        if (user == null)
            return Results.BadRequest("Invalid username or password");

        var passwordHasher = new PasswordHasher<Data.User>();
        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.password);

        if (result == PasswordVerificationResult.Failed)
            return Results.BadRequest("Invalid username or password");

        var token = tokenService.CreateToken(user);
        var refreshToken = tokenService.GenerateRefreshToken();
        
        user.LastVisit = DateTime.UtcNow;
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        _dbContext.Update(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(new Login.Response(token, refreshToken));
    }
}
