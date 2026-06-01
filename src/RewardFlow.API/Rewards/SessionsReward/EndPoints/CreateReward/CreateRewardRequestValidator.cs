using FluentValidation;

namespace Reward_Flow_v2.Rewards.SessionsReward.CreateReward;

public static partial class CreateSessionsReward
{
    public class CreateRewardRequestValidator : AbstractValidator<Request>
    {
        public CreateRewardRequestValidator()
        {

            RuleFor(x => x.Name)
                .NotEmpty()
                .When(x => x.Name is not null);

            RuleFor(x => x.Code)
                .NotEmpty()
                .MaximumLength(50)
                .When(x => x.Code is not null);

            RuleFor(x => x.Year)
                .GreaterThan((short)2011)
                .When(x => x.Year is not null);

            RuleFor(x => x.Semester)
                .GreaterThan((byte)0)
                .When(x => x.Semester is not null);

            RuleFor(x => x.Percentage)
                .GreaterThan(0);
        }
    }
}