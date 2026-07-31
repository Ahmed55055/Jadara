using Microsoft.EntityFrameworkCore;
using Reward_Flow_v2.Common.EndpointValidation;
using Reward_Flow_v2.Rewards.Data.Database;
using RewardFlow_API.Rewards.Data;

namespace RewardFlow_API.Rewards.Courses;

public static class GetCourseById
{
    public static void MapGetCourseById(this IEndpointRouteBuilder app)
    {
        app.MapGet(CourseApiPath.CourseById, HandlerAsync)
            .RequireAuthorization()
            .WithName(CourseApiPath.CourseById)
            .Produces<CourseResponseDto>()
            .Produces(StatusCodes.Status404NotFound)
            .WithTags(CourseApiPath.Tag)
            .ValidateAccess();
    }

    private static async Task<IResult> HandlerAsync(int id, RewardDbContext context,
        CancellationToken cancellationToken)
    {
        var course = await context.Course
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        return course is null ? Results.NotFound() : Results.Ok(MapToDto(course));
    }

    private static CourseResponseDto MapToDto(Course course)
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