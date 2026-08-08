using FluentValidation;
using Reward_Flow_v2.Common.EndpointValidation;
using Reward_Flow_v2.Rewards.Data.Database;
using RewardFlow_API.Rewards.Data;

namespace RewardFlow_API.Rewards.Courses.Create;
public static class CreateCourse
{
    public static void MapCreateCourse(this IEndpointRouteBuilder app)
    {
        app.MapPost(CourseApiPath.Courses, HandlerAsync)
            .RequireAuthorization()
            .Produces<CourseResponseDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithTags(CourseApiPath.Tag)
            .Validation(new ValidateCourseCreateRequest())
            .ValidateAccess();
    }

    private static async Task<IResult> HandlerAsync(CourseRequestDto dto, RewardDbContext context,
        ILogger<Course> logger, CancellationToken cancellationToken)
    {
        Course course = MapToCourse(dto);

        try
        {
            context.Course.Add(course);
            await context.SaveChangesAsync(cancellationToken);
            return Results.CreatedAtRoute(CourseApiPath.CourseById, new { id = course.Id }, course);
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while adding a new course {@Course}", course);
            return Results.Problem("An error occurred while adding a new course",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static Course MapToCourse(CourseRequestDto dto)
    {
        return new Course
        {
            Name = dto.Name,
            Code = dto.Code,
            IsTheoretical = dto.IsTheoretical,
            IsPractical = dto.IsPractical,
            SubjectPrice = dto.SubjectPrice
        };
    }
}

public class ValidateCourseCreateRequest : AbstractValidator<CourseRequestDto>
{
    public ValidateCourseCreateRequest()
    {
        RuleFor(c => c.Name)
            .NotNull()
            .NotEmpty();
    }
}

public record CourseRequestDto(string Name, string? Code, bool IsTheoretical, bool IsPractical, decimal SubjectPrice);