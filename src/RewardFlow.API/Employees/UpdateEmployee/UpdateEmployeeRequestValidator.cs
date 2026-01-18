using FluentValidation;
using static Reward_Flow_v2.Employees.UpdateEmployee.UpdateEmployee;

namespace Reward_Flow_v2.Employees.UpdateEmployee;

public class UpdateEmployeeRequestValidator : AbstractValidator<Request>
{
    public UpdateEmployeeRequestValidator()
    {
        RuleFor(x => x.Salary.Value).GreaterThanOrEqualTo(0).When(x => x.Salary.HasValue);
    }
}