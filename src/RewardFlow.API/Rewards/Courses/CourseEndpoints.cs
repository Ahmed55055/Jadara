using RewardFlow_API.Rewards.Courses.Create;
using RewardFlow_API.Rewards.Courses.Get;
using RewardFlow_API.Rewards.Courses.Update;
using RewardFlow_API.Rewards.Courses.Delete;

namespace RewardFlow_API.Rewards.Courses;

public static class CourseEndpoints
{
    public static void MapCourseEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapCreateCourse();
        app.MapGetCourseById();
        app.MapGetAllCourses();
        app.MapUpdateCourse();
        app.MapDeleteCourse();
    }
}