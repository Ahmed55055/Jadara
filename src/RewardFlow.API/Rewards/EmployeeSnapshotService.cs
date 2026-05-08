using Microsoft.EntityFrameworkCore;
using Reward_Flow_v2.Employees.Data;
using Reward_Flow_v2.Rewards.Data;
using Reward_Flow_v2.Rewards.Data.Database;

namespace Reward_Flow_v2.Rewards;

public class EmployeeSnapshotService : ISnapshotService<Employee, EmployeeSnapshot>
{
    private readonly RewardDbContext _context;

    public EmployeeSnapshotService(RewardDbContext context)
    {
        _context = context;
    }

    public Task<EmployeeSnapshot> Capture(Employee entity)
    {
        var snapshot = MapToSnapshot(entity);
        _context.EmployeeSnapshots.Add(snapshot);
        return Task.FromResult(snapshot);
    }

    public Task<IEnumerable<EmployeeSnapshot>> Capture(IEnumerable<Employee> entities)
    {
        var snapshots = entities.Select(MapToSnapshot).ToList();
        _context.EmployeeSnapshots.AddRange(snapshots);
        return Task.FromResult<IEnumerable<EmployeeSnapshot>>(snapshots);
    }

    public async Task<EmployeeSnapshot?> GetLatestSnapshot(Employee entity)
    {
        return await _context.EmployeeSnapshots
            .Where(s => s.EmployeeId == entity.EmployeeId)
            .OrderByDescending(s => s.SnapshotDate)
            .FirstOrDefaultAsync();
    }

    public async Task<EmployeeSnapshot> EnsureLatest(Employee entity)
    {
        var latest = await GetLatestSnapshot(entity);

        if (latest != null && IsUpToDate(entity, latest))
            return latest;

        return await Capture(entity);
    }

    public async Task<IEnumerable<EmployeeSnapshot>> EnsureLatest(IEnumerable<Employee> entities)
    {
        var employeeIds = entities.Select(e => e.EmployeeId).ToList();

        var latestSnapshots = await _context.EmployeeSnapshots
            .Where(s => employeeIds.Contains(s.EmployeeId))
            .GroupBy(s => s.EmployeeId)
            .Select(g => g.OrderByDescending(s => s.SnapshotDate).First())
            .ToDictionaryAsync(s => s.EmployeeId);

        var results = new List<EmployeeSnapshot>();
        var newSnapshots = new List<EmployeeSnapshot>();

        foreach (var entity in entities)
        {
            if (latestSnapshots.TryGetValue(entity.EmployeeId, out var latest) && IsUpToDate(entity, latest))
            {
                results.Add(latest);
            }
            else
            {
                var fresh = MapToSnapshot(entity);
                newSnapshots.Add(fresh);
                results.Add(fresh);
            }
        }

        if (newSnapshots.Count != 0)
        {
            _context.EmployeeSnapshots.AddRange(newSnapshots);
        }

        return results;
    }

    private EmployeeSnapshot MapToSnapshot(Employee entity)
    {
        return new EmployeeSnapshot
        {
            // SnapshotId and SnapshotDate should ideally be set in the 
            // EmployeeSnapshot constructor or via EF default values
            EmployeeId = entity.EmployeeId,
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