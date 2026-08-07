using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Reward_Flow_v2.Common.EndpointValidation;
using Reward_Flow_v2.User.Data;
using Reward_Flow_v2.User.Data.Database;
using RewardFlow_API.User.AuthService;
using RewardFlow_API.User.Data.Dtos;

namespace Reward_Flow_v2.User.AuthService.Register;

public static class Register
{
    public record Request(string username, string password, string? email);

    public record Response(UserDto User, string JwtToken, string RefreshToken);

    public static void MapRegisterUser(this IEndpointRouteBuilder app)
    {
        app.MapPost(AuthApiPath.Register, HandlerAsync)
            .Produces<Response>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status409Conflict)
            .Produces<IEnumerable<FluentValidation.Results.ValidationFailure>>(StatusCodes.Status400BadRequest)
            .WithTags(AuthApiPath.Tag)
            .Validation(new RegisterUserRequestValidator());
    }

    private static async Task<IResult> HandlerAsync(Request request, UserDbContext _dbContext,
        TokenService tokenService,
        CancellationToken cancellationToken)
    {
        var IsUsernameInUse = await _dbContext.User.AnyAsync(x => x.Username == request.username);

        if (IsUsernameInUse)
            return Results.Conflict(request.username);
        var refreshToken = tokenService.GenerateRefreshToken();
        var user = PrepareNewUserObject(request.username, request.password, refreshToken, request.email);

        try
        {
            _dbContext.User.Add(user);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception)
        {
            return Results.InternalServerError();
        }

        var userDto = new UserDto(user.UUID, user.Username, user.Email, user.CreatedAt, user.IsActive);
        var response = new Response(userDto, tokenService.CreateToken(user), refreshToken);

        return Results.Created(string.Empty, response);
    }

    private static Data.User PrepareNewUserObject(string username, string password, string refreshToken, string? email)
    {
        Data.User user = new();

        var passwordHash = new PasswordHasher<Data.User>()
            .HashPassword(user, password);

        user.PrepareNewUser(username, passwordHash, refreshToken, email);

        return user;
    }
}