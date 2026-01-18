using FluentValidation;
using static Reward_Flow_v2.User.AuthService.Login.Login;

namespace Reward_Flow_v2.User.AuthService.Login;


public class LoginUserRequestValidator : AbstractValidator<Login.Request>
{
    public LoginUserRequestValidator()
    {
        RuleFor(x => x.username).NotEmpty().WithMessage("Username is required");
        RuleFor(x => x.password).NotEmpty().WithMessage("Password is required");
    }

}

