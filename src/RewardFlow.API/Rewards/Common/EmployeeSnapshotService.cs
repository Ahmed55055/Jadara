using Microsoft.EntityFrameworkCore;
using Reward_Flow_v2.Employees.Data;
using Reward_Flow_v2.Rewards;
using Reward_Flow_v2.Rewards.Data;
using Reward_Flow_v2.Rewards.Data.Database;
using System.Linq.Expressions;

namespace RewardFlow_API.Rewards.Common;

public class EmployeeSnapshotService : ISnapshotService<Employee, EmployeeSnapshot>
{
    private readonly RewardDbContext _context;

    public EmployeeSnapshotService(RewardDbContext context)
    {
        _context = context;
    }

    public EmployeeSnapshot Capture(Employee entity)
    {
        var snapshot = MapToSnapshot(entity);
        _context.EmployeeSnapshots.Add(snapshot);
        return snapshot;
    }

    public IEnumerable<EmployeeSnapshot> Capture(IEnumerable<Employee> entities)
    {
        var snapshots = entities.Select(MapToSnapshot).ToList();
        return snapshots;
    }

    public IQueryable<EmployeeSnapshot> GetLatestSnapshot(int semesterSubjectId)
    {
        return _context.EmployeeSnapshots
            .Where(s => s.EmployeeId == semesterSubjectId)
            .OrderByDescending(s => s.SnapshotDate);
    }

    public IQueryable<EmployeeSnapshot> GetLatestSnapshot(Expression<Func<EmployeeSnapshot, bool>> predicate)
    {
        return _context.EmployeeSnapshots
            .Where(predicate)
            .AsQueryable();
    }

    public IQueryable<IEnumerable<EmployeeSnapshot>> GetLatestSnapshot(int[] entityIds)
    {
        return _context.EmployeeSnapshots
            .Where(s => entityIds.Contains(s.EmployeeId))
            .GroupBy(s => s.EmployeeId)
            .Select(g => g.OrderByDescending(s => s.SnapshotDate).Take(1));
    }

    public bool Compare(EmployeeSnapshot snapshot, Employee entity)
    {
        if (snapshot is null || entity is null)
            return false;

        var entitySnapshot = Capture(entity);
        return Compare(snapshot, entitySnapshot);
    }

    public bool Compare(EmployeeSnapshot? snapshot, EmployeeSnapshot? other)
    {
        if (snapshot is null || other is null)
            return false;

        // Kept simple to avoid increasing the surface area that needs to be maintained.
        return snapshot.EmployeeId == other.EmployeeId
               && snapshot.Name == other.Name
               && snapshot.NationalNumber == other.NationalNumber
               && snapshot.AccountNumber == other.AccountNumber
               && snapshot.Salary == other.Salary
               && snapshot.JobTitle == other.JobTitle;
    }

    private EmployeeSnapshot MapToSnapshot(Employee entity)
    {
        return new EmployeeSnapshot
        {
            // SnapshotId and SnapshotDate should ideally be set in the 
            // EmployeeSnapshot constructor or via EF default values
            EmployeeId = entity.Id,
            Name = entity.Name,
            NationalNumber = entity.NationalNumber,
            AccountNumber = entity.AccountNumber,
            Salary = entity.Salary,
            JobTitle = entity.JobTitle,
            // Hashes are handled automatically by the properties in EmployeeSnapshot
        };
    }

    private bool IsUpToDate(Employee entity, EmployeeSnapshot snapshot)
    {
        // Compare hashes and critical fields to see if a new snapshot is needed
        return entity.NationalNumberHash == snapshot.NationalNumberHash &&
               entity.AccountNumberHash == snapshot.AccountNumberHash &&
               entity.Salary == snapshot.Salary &&
               entity.Name == snapshot.Name &&
               entity.JobTitle == snapshot.JobTitle;
    }
}