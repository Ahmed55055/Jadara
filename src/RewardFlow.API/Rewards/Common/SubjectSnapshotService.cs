using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Reward_Flow_v2.Rewards;
using Reward_Flow_v2.Rewards.Data;
using Reward_Flow_v2.Rewards.Data.Database;
using RewardFlow_API.Rewards.Data;

namespace RewardFlow_API.Rewards.Common;

public class SubjectSnapshotService(RewardDbContext context) : ISnapshotService<TermCourse, CourseSnapshot>
{
    public CourseSnapshot Capture(TermCourse entity)
    {
        var snapshot = MapToSnapshot(entity);
        context.CourseSnapshot.Add(snapshot);
        return snapshot;
    }

    public IEnumerable<CourseSnapshot> Capture(IEnumerable<TermCourse> entities)
    {
        var snapshots = entities.Select(e => Capture(e)).ToList();
        return snapshots;
    }

    public IQueryable<CourseSnapshot> GetLatestSnapshot(int semesterSubjectId)
    {
        return context.CourseSnapshot
            .Where(s => s.SemesterSubjectId == semesterSubjectId)
            .OrderByDescending(s => s.CapturedAt);
    }

    public IQueryable<IEnumerable<CourseSnapshot>> GetLatestSnapshot(int[] entityIds)
    {
        return context.CourseSnapshot
            .Where(s => entityIds.Contains(s.SemesterSubjectId))
            .GroupBy(s => s.SemesterSubjectId)
            .Select(g => g.OrderByDescending(s => s.CapturedAt).Take(1));
    }

    private bool IsUpToDate(TermCourse entity, CourseSnapshot latest)
    {
        // Compare the core values. If any differ, the snapshot is out of date.
        // Note: We access entity.Subject properties assuming they were Included in the query.
        return entity.SubjectId == latest.SemesterSubjectId &&
               entity.Course.Name == latest.SubjectName &&
               entity.Course.IsTheoretical == latest.IsTheoretical &&
               entity.Course.IsPractical == latest.IsPractical &&
               entity.Semester == latest.Semester &&
               entity.Year == latest.Year;
    }


    private CourseSnapshot MapToSnapshot(TermCourse entity)
    {
        return new CourseSnapshot
        {
            SubjectName = entity.Course.Name,
            IsTheoretical = entity.Course.IsTheoretical,
            IsPractical = entity.Course.IsPractical,
            Semester = entity.Semester,
            Year = entity.Year,
            TermCourse = entity
        };
    }
}