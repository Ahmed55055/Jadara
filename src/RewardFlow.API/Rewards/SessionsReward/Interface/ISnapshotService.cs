using Reward_Flow_v2.Rewards.Data;
using Z.EntityFramework.Plus;

namespace Reward_Flow_v2.Rewards;

public interface ISnapshotService<in T, TSnapshot> where T : class
{
    public TSnapshot Capture(T entity);
    public IEnumerable<TSnapshot> Capture(IEnumerable<T> entities);
    public IQueryable<TSnapshot> GetLatestSnapshot(int entityId);
    public IQueryable<IEnumerable<TSnapshot>> GetLatestSnapshot(int[] entityIds);
}