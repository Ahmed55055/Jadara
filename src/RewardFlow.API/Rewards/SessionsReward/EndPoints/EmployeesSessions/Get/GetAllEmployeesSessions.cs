using Reward_Flow_v2.Common.EndpointValidation;
using RewardFlow_API.Rewards.SessionsReward.EndPoints.CourseAssignments.Create;

namespace Reward_Flow_v2.Rewards.SessionsReward.EndPoints.Employees.GetAllEmployees;

public static class GetAllEmployeesSessions
{
    public static void MapGetAllEmployeesSessions(this IEndpointRouteBuilder app)
    {
        app.MapGet(RewardApiPath.EmployeeSessions, HandlerAsync)
            .RequireAuthorization()
            .Produces<EmployeeSessionsDto>()
            .Produces(StatusCodes.Status404NotFound)
            .WithTags(RewardApiPath.Tag)
            .ValidateAccess();
    }

    private static async Task<IResult> HandlerAsync(int rewardId, ISessionRewardService service,
        IHttpContextAccessor httpContextAccessor, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}