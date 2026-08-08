using Microsoft.EntityFrameworkCore;
using Reward_Flow_v2.Common.EndpointValidation;
using Reward_Flow_v2.Rewards.Data.Database;
using RewardFlow_API.Rewards.Data;

namespace RewardFlow_API.Rewards.Courses.Get;

public static class GetAllCourses
{
    public static void MapGetAllCourses(this IEndpointRouteBuilder app)
    {
        app.MapGet(CourseApiPath.Courses, HandlerAsync)
            .RequireAuthorization()
            .WithName(CourseApiPath.Courses)
            .Produces<List<CourseResponseDto>>()
            .Produces(StatusCodes.Status401Unauthorized)
            .WithTags(CourseApiPath.Tag)
            .ValidateAccess();
    }

    private static async Task<IResult> HandlerAsync(RewardDbContext context, CancellationToken cancellationToken)
    {
        var courses = await context.Course
            .AsNoTracking()
            .Select(c => MapCourseToResponse(c))
            .ToListAsync(cancellationToken);

        return Results.Ok(courses);
    }
 
    private static CourseResponseDto MapCourseToResponse(Course course)
    {
        return new CourseResponseDto(
            course.Id,
            course.Code,
            course.Name,
            course.IsTheoretical,
            course.IsPractical,
            course.SubjectPrice);
    }
}