using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Reward_Flow_v2.User.Data;
using Reward_Flow_v2.User.Data.Database;
using RewardFlow_API.User.Data.Dtos;

namespace Reward_Flow_v2.User.AuthService.Register;

public static class Register
{
    public record Request(string username, string password, string? email);

    public record Response(UserDto User, string JwtToken, string RefreshToken);

    public static void MapRegisterUser(this IEndpointRouteBuilder app)
    {
        app.MapPost(AuthApiPath.Register, HandlerAsync)
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status409Conflict)
            .Produces<IEnumerable<FluentValidation.Results.ValidationFailure>>(StatusCodes.Status400BadRequest)
            .WithTags(AuthApiPath.Tag);
    }

    private static async Task<IResult> HandlerAsync(Request request, UserDbContext _dbContext,
        CancellationToken cancellationToken)
    {
        var ValidateRequest = new RegisterUserRequestValidator().Validate(request);

        if (!ValidateRequest.IsValid)
            return Results.BadRequest(ValidateRequest.Errors);

        var IsUsernameInUse = await _dbContext.User.AnyAsync(x => x.Username == request.username);

        if (IsUsernameInUse)
            return Results.Conflict(request.username);

        var user = PrepareNewUserObject(request.username, request.password, request.email);

        try
        {
            _dbContext.User.Add(user);
            await _dbContext.SaveChangesAsync();

            return Results.Created();
        }
        catch (Exception)
        {
            return Results.InternalServerError();
        }
    }

    private static Data.User PrepareNewUserObject(string username, string password, string? email)
    {
        Data.User user = new();

        var passwordHash = new PasswordHasher<Data.User>()
            .HashPassword(user, password);

        user.PrepareNewUser(username, passwordHash, email);

        return user;
    }
}