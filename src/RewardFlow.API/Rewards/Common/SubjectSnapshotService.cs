using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Reward_Flow_v2.Rewards;
using Reward_Flow_v2.Rewards.Data;
using Reward_Flow_v2.Rewards.Data.Database;
using RewardFlow_API.Rewards.Data;

namespace RewardFlow_API.Rewards.Common;

public class SubjectSnapshotService(RewardDbContext context) : ISnapshotService<SemesterSubject, SubjectSnapshot>
{
    public SubjectSnapshot Capture(SemesterSubject entity)
    {
        var snapshot = MapToSnapshot(entity);
        context.SubjectSnapshot.Add(snapshot);
        return snapshot;
    }

    public IEnumerable<SubjectSnapshot> Capture(IEnumerable<SemesterSubject> entities)
    {
        var snapshots = entities.Select(e => Capture(e)).ToList();
        return snapshots;
    }

    public IQueryable<SubjectSnapshot> GetLatestSnapshot(int semesterSubjectId)
    {
        return context.SubjectSnapshot
            .Where(s => s.SemesterSubjectId == semesterSubjectId)
            .OrderByDescending(s => s.CapturedAt);
    }

    public IQueryable<IEnumerable<SubjectSnapshot>> GetLatestSnapshot(int[] entityIds)
    {
        return context.SubjectSnapshot
            .Where(s => entityIds.Contains(s.SemesterSubjectId))
            .GroupBy(s => s.SemesterSubjectId)
            .Select(g => g.OrderByDescending(s => s.CapturedAt).Take(1));
    }

    private bool IsUpToDate(SemesterSubject entity, SubjectSnapshot latest)
    {
        // Compare the core values. If any differ, the snapshot is out of date.
        // Note: We access entity.Subject properties assuming they were Included in the query.
        return entity.SubjectId == latest.SemesterSubjectId &&
               entity.Subject.Name == latest.SubjectName &&
               entity.Subject.IsTheoretical == latest.IsTheoretical &&
               entity.Subject.IsPractical == latest.IsPractical &&
               entity.Semester == latest.Semester &&
               entity.Year == latest.Year;
    }


    private SubjectSnapshot MapToSnapshot(SemesterSubject entity)
    {
        return new SubjectSnapshot
        {
            SubjectName = entity.Subject.Name,
            IsTheoretical = entity.Subject.IsTheoretical,
            IsPractical = entity.Subject.IsPractical,
            Semester = entity.Semester,
            Year = entity.Year,
            SemesterSubject = entity
        };
    }
}