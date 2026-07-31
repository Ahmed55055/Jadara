using Reward_Flow_v2;

namespace RewardFlow_API.Rewards.Courses;

public class CourseApiPath
{
    public const string Tag = "courses";
    private const string CourseRootApi = $"{ApiPath.Route}/{Tag}";
    
    public const string Courses = CourseRootApi;
    public const string CourseById = $"{CourseRootApi}/{{id}}";
}