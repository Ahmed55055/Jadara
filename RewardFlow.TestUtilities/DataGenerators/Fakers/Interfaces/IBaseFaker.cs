using Bogus;

namespace RewardFlow.TestUtilities.DataGenerators.Fakers.Employees;

public interface IBaseFaker<TEntity> :IRuleSet<TEntity> where TEntity : class
{
    public TEntity Generate(string ruleSets = null);
    public List<TEntity> Generate(int count, string ruleSets = null);
}