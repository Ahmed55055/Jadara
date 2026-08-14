using FluentValidation;

namespace RewardFlow_API.Rewards.SessionsReward.EndPoints.RewardManage.UpdateSessionsReward;
internal class UpdateSessionsRewardValidator : AbstractValidator<UpdateSessionsRewardRequest>
{
    public UpdateSessionsRewardValidator()
    {
        RuleFor(x => x.RewardName.Value)
            .NotEmpty()
            .When(x => x.RewardName.HasValue);

        RuleFor(x => x.Percentage.Value)
            .GreaterThan(0)
            .When(x => x.Percentage.HasValue);
    }
}