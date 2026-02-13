using Reward_Flow_v2.Employees.Data;
using System.Linq.Expressions;

namespace RewardFlow.TestUtilities.DataGenerators.Fakers.Employees;

public interface IEntityFaker<TEntity, in TFlags>: IBaseFaker<TEntity> where TEntity : class
{
    public IEntityFaker<TEntity,TFlags> WithNulls(TFlags fields);
    public IEntityFaker<TEntity,TFlags> ForProperty<TProperty>(Expression<Func<TEntity, TProperty>> property, TProperty value);
    public IEntityFaker<TEntity,TFlags> WithValue(TFlags fields);
}