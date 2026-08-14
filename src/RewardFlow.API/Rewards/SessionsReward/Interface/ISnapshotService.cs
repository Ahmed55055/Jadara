using Reward_Flow_v2.Employees.Data;
using Reward_Flow_v2.Rewards.Data;
using System.Linq.Expressions;
using Z.EntityFramework.Plus;

namespace Reward_Flow_v2.Rewards;

public interface ISnapshotService<in T, TSnapshot> where T : class
{
    public TSnapshot Capture(T entity);
    public IEnumerable<TSnapshot> Capture(IEnumerable<T> entities);
    public IQueryable<TSnapshot> GetLatestSnapshot(int entityId);
    public IQueryable<TSnapshot> GetLatestSnapshot(Expression<Func<TSnapshot, bool>> predicate);
    public IQueryable<IEnumerable<TSnapshot>> GetLatestSnapshot(int[] entityIds);
    bool Compare(TSnapshot snapshot, T entity);
    bool Compare(TSnapshot? snapshot, TSnapshot? other);
}