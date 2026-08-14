using Reward_Flow_v2.Common.EndpointValidation;

namespace Reward_Flow_v2.Rewards.SessionsReward.EndPoints.Employees.GetAllEmployees;

public static class GetEmployeeSessionsByEmployeeId
{
    public static void MapGetEmployeeSessionsById(this IEndpointRouteBuilder app)
    {
        app.MapGet(RewardApiPath.EmployeeSessionsById, HandlerAsync)
            .RequireAuthorization()
            .Produces<List<EmployeeSessionsDto>>()
            .Produces(StatusCodes.Status404NotFound)
            .WithTags(RewardApiPath.Tag)
            .ValidateAccess();
    }

    private static async Task<IResult> HandlerAsync(int rewardId, int employeeId, ISessionRewardService service,
        IHttpContextAccessor httpContextAccessor, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}