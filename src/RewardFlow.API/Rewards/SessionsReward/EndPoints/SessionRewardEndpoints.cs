using Reward_Flow_v2.Rewards.SessionsReward.EndPoints.CourseAssignments.Remove;
using Reward_Flow_v2.Rewards.SessionsReward.EndPoints.CourseAssignments.Update;
using Reward_Flow_v2.Rewards.SessionsReward.EndPoints.Employees.GetAllEmployees;
using RewardFlow_API.Rewards.Courses.Get;
using RewardFlow_API.Rewards.SessionsReward.EndPoints.CourseAssignments.Create;
using RewardFlow_API.Rewards.SessionsReward.EndPoints.CourseAssignments.Get;
using RewardFlow_API.Rewards.SessionsReward.EndPoints.RewardManage.CreateReward;
using RewardFlow_API.Rewards.SessionsReward.EndPoints.RewardManage.DeleteSessionsReward;
using RewardFlow_API.Rewards.SessionsReward.EndPoints.RewardManage.GetAllSessionsRewards;
using RewardFlow_API.Rewards.SessionsReward.EndPoints.RewardManage.GetSessionsRewardById;
using RewardFlow_API.Rewards.SessionsReward.EndPoints.RewardManage.UpdateSessionsReward;

namespace Reward_Flow_v2.Rewards.SessionsReward.EndPoints;

public static class SessionRewardEndpoints
{
    public static void MapSessionRewardEndpoints(this IEndpointRouteBuilder app)
    {
        // --------- Reward ---------
        app.MapCreateSessionsReward();
        app.MapGetAllSessionsRewards();
        app.MapGetSessionsRewardById();
        app.MapUpdateSessionsReward();
        app.MapDeleteSessionsReward();
        
       app.MapCourseAssignmentEndpoints();
       app.MapEmployeeSessionsEndpoints();
    }

    private static void MapCourseAssignmentEndpoints(this IEndpointRouteBuilder app)
    {
        //  -- add --
        app.MapAddCourseAssignments();
        
        // -- get --
        app.MapGetCourseAssignment();
        app.MapGetAllCourseAssignments();
        
        // -- update --
        app.MapUpdateCourseAssignment();
        
        // -- delete --
        app.MapRemoveCourseAssignment();

    }

    private static void MapEmployeeSessionsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGetAllEmployeesSessions();
        app.MapGetEmployeeSessionsById();
    }

}