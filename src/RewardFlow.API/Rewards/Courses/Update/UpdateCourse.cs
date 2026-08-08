using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Reward_Flow_v2.Common.EndpointValidation;
using Reward_Flow_v2.Rewards.Data.Database;
using RewardFlow_API.Rewards.Courses.Create;
using RewardFlow_API.Rewards.Data;

namespace RewardFlow_API.Rewards.Courses.Update;

public static class UpdateCourse
{
    public static void MapUpdateCourse(this IEndpointRouteBuilder app)
    {
        app.MapPut(CourseApiPath.CourseById, HandlerAsync)
            .RequireAuthorization()
            .Produces<CourseResponseDto>()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithTags(CourseApiPath.Tag)
            .Validation(new ValidateCourseUpdateRequest())
            .ValidateAccess();
    }

    private static async Task<IResult> HandlerAsync(int id, CourseRequestDto dto, RewardDbContext context,
        ILogger<Course> logger, CancellationToken cancellationToken)
    {
        try
        {
            var course = await context.Course
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

            if (course is null)
            {
                return Results.NotFound();
            }

            course.Name = dto.Name;
            course.Code = dto.Code;
            course.IsTheoretical = dto.IsTheoretical;
            course.IsPractical = dto.IsPractical;
            course.SubjectPrice = dto.SubjectPrice;

            await context.SaveChangesAsync(cancellationToken);

            return Results.Ok(MapToDto(course));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while updating the course with id {CourseId}", id);
            return Results.Problem("An error occurred while updating the course", statusCode: 500);
        }
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

public class ValidateCourseUpdateRequest : AbstractValidator<CourseRequestDto>
{
    public ValidateCourseUpdateRequest()
    {
        RuleFor(c => c.Name)
            .NotNull()
            .NotEmpty();
    }
}
