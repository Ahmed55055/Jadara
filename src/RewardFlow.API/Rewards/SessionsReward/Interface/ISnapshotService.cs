namespace Reward_Flow_v2.Rewards;

public interface ISnapshotService<in T, TSnapshot> where T: class
{
    public Task<TSnapshot> Capture(T entity);
    public Task<IEnumerable<TSnapshot>> Capture(IEnumerable<T> entities);
    public Task<TSnapshot?> GetLatestSnapshot (T entity);
    public Task<TSnapshot> EnsureLatest(T entity);
    public Task<IEnumerable<TSnapshot>> EnsureLatest(IEnumerable<T> entities);
}