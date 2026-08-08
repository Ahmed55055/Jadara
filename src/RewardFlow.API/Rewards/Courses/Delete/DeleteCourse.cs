using Microsoft.EntityFrameworkCore;
using Reward_Flow_v2.Common.EndpointValidation;
using Reward_Flow_v2.Rewards.Data.Database;

namespace RewardFlow_API.Rewards.Courses.Delete;

public static class DeleteCourse
{
    public static void MapDeleteCourse(this IEndpointRouteBuilder app)
    {
        app.MapDelete(CourseApiPath.CourseById, HandlerAsync).RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithTags(CourseApiPath.Tag)
            .ValidateAccess();
    }

    public static async Task<IResult> HandlerAsync(int id, RewardDbContext context, CancellationToken cancellationToken)
    {
        
            var course = await context.Course.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
            if (course == null) { return Results.NotFound(); }

            bool hasReferences = await context.TermCourse.AnyAsync(tc => tc.CourseId == id, cancellationToken);
            if (hasReferences)
            {
                return Results.Conflict("Cannot delete a course that is assigned to subject semesters.");
            }

            context.Course.Remove(course);
            await context.SaveChangesAsync(cancellationToken);
            return Results.NoContent();
        
       
    }
}