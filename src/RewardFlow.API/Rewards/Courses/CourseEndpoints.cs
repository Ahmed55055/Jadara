namespace RewardFlow_API.Rewards.Courses;

public static class CourseEndpoints
{
    public static void MapCourseEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapCreateCourse();
        app.MapGetCourseById();
    }
}