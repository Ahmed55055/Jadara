using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Reward_Flow_v2.Rewards;
using Reward_Flow_v2.Rewards.Data;
using Reward_Flow_v2.Rewards.Data.Database;
using RewardFlow_API.Rewards.Data;
using System.Linq.Expressions;

namespace RewardFlow_API.Rewards.Common;

public class CourseSnapshotService(RewardDbContext context) : ISnapshotService<TermCourse, CourseSnapshot>
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
            .Where(s => s.TermCourseId == semesterSubjectId)
            .OrderByDescending(s => s.CapturedAt);
    }

    public IQueryable<CourseSnapshot> GetLatestSnapshot(Expression<Func<CourseSnapshot, bool>> predicate)
    {
        return context.CourseSnapshot
            .Where(predicate)
            .AsQueryable();
    }

    public IQueryable<IEnumerable<CourseSnapshot>> GetLatestSnapshot(int[] entityIds)
    {
        return context.CourseSnapshot
            .Where(s => entityIds.Contains(s.TermCourseId))
            .GroupBy(s => s.TermCourseId)
            .Select(g => g.OrderByDescending(s => s.CapturedAt).Take(1));
    }

    public bool Compare(CourseSnapshot snapshot, TermCourse entity)
    {
        if (snapshot is null || entity is null)
            return false;

        var entitySnapshot = Capture(entity);
        return Compare(snapshot, entitySnapshot);
    }

    public bool Compare(CourseSnapshot? snapshot, CourseSnapshot? other)
    {
        if (snapshot is null || other is null)
            return false;
        
        return snapshot.CourseId == other.CourseId
               && snapshot.TermCourseId == other.TermCourseId
               && snapshot.CourseName == other.CourseName
               && snapshot.StudentCount == other.StudentCount
               && snapshot.IsTheoretical == other.IsTheoretical
               && snapshot.IsPractical == other.IsPractical
               && snapshot.Term == other.Term
               && snapshot.Year == other.Year;
    }

    private bool IsUpToDate(TermCourse entity, CourseSnapshot latest)
    {
        // Compare the core values. If any differ, the snapshot is out of date.
        // Note: We access entity.Subject properties assuming they were Included in the query.
        return entity.CourseId == latest.TermCourseId &&
               entity.Course.Name == latest.CourseName &&
               entity.Course.IsTheoretical == latest.IsTheoretical &&
               entity.Course.IsPractical == latest.IsPractical &&
               entity.Term == latest.Term &&
               entity.Year == latest.Year;
    }


    private CourseSnapshot MapToSnapshot(TermCourse entity)
    {
        return new CourseSnapshot
        {
            CourseName = entity.Course.Name,
            IsTheoretical = entity.Course.IsTheoretical,
            IsPractical = entity.Course.IsPractical,
            StudentCount = entity.StudentCount,
            Term = entity.Term,
            Year = entity.Year,
            CourseId = entity.CourseId,
            TermCourse = entity
        };
    }
}