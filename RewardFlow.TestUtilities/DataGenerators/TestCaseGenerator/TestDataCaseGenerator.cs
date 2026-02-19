using RewardFlow.TestUtilities.DataGenerators.Fakers.Employees;
using RewardFlow.TestUtilities.Extentions;
using System.Linq.Expressions;

namespace RewardFlow_UnitTest.Employees;

public static class TestDataCaseGenerator
{
    // This method can "see" the types inside the fakerFactory automatically!
    public static TestDataCaseGenerator<TEntity, TFlag> Create<TEntity, TFlag>(
        Func<IEntityFaker<TEntity, TFlag>> fakerFactory)
        where TEntity : class
    {
        return new TestDataCaseGenerator<TEntity, TFlag>(fakerFactory);
    }
}

/// <summary>
/// A fluent builder designed to orchestrate the generation of strongly-typed test data cases.
/// </summary>
/// <typeparam name="TEntity">The type of the entity being generated.</typeparam>
/// <typeparam name="TFlag">A configuration flag type used by the underlying faker.</typeparam>
/// <remarks>
/// This class facilitates Data-Driven Testing (DDT) by mapping specific property values 
/// to descriptive test case labels, ensuring type safety via Expression Trees.
/// </remarks>
public class TestDataCaseGenerator<TEntity, TFlag>(Func<IEntityFaker<TEntity, TFlag>> fakerFactory)
    where TEntity : class
{
    private readonly List<Func<IEntityFaker<TEntity, TFlag>, IEnumerable<object[]>>> _piplines = [];

    /// <summary>
    /// Internally processes the property expression and maps test cases into a format 
    /// compatible with xUnit/NUnit <c>MemberData</c> or <c>TestCaseSource</c>.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property being targeted.</typeparam>
    /// <param name="faker">The specific faker instance for this execution pipe.</param>
    /// <param name="cases">A collection of value-description pairs to be tested.</param>
    /// <param name="property">An expression identifying the target property on the entity.</param>
    /// <returns>
    /// An enumerable of object arrays where:
    /// <list type="bullet">
    /// <item><description><c>[0]</c>: The Entity instance.</description></item>
    /// <item><description><c>[1]</c>: A <see cref="TestCaseInfo"/> object containing the metadata for the test.</description></item>
    /// </list>
    /// </returns>
    private static IEnumerable<object[]> GenerateInternal<TProperty>(IEntityFaker<TEntity, TFlag> faker,
        IEnumerable<(TProperty value, string discription)> cases, Expression<Func<TEntity, TProperty>> property)
    {
        string propertyName = property.GetPropertyName();

        foreach (var @case in cases)
        {
            var entityWithAppliedValue = faker.ForProperty(property, @case.value).Generate();
            var testCaseInfo = new TestCaseInfo(entityWithAppliedValue, propertyName, @case.value, @case.discription);
            
            yield return
                [entityWithAppliedValue, testCaseInfo];
        }
    }

    /// <summary>
    /// Registers a new set of test cases for a specific property.
    /// </summary>
    /// <typeparam name="TProprity">The type of the property being configured.</typeparam>
    /// <param name="cases">The data points and descriptions to generate.</param>
    /// <param name="property">The property selector (e.g., <c>x => x.Id</c>).</param>
    /// <returns>The current <see cref="TestDataCaseGenerator{TEntity, TFlag}"/> instance for method chaining.</returns>
    public TestDataCaseGenerator<TEntity, TFlag> AddCases<TProprity>(
        IEnumerable<(TProprity value, string discription)> cases, Expression<Func<TEntity, TProprity>> property)
    {
        _piplines.Add(faker => (GenerateInternal(faker, cases, property)));
        return this;
    }

    /// <summary>
    /// Executes all registered pipelines and flattens their results into a single stream of test cases.
    /// </summary>
    /// <remarks>
    /// This method iterates through every configured pipeline, initializes a new faker instance 
    /// for each, and yields the resulting object arrays for use in data-driven tests.
    /// </remarks>
    /// <returns>
    /// An <see cref="IEnumerable{T}"/> of <see cref="object"/> arrays, where each array represents 
    /// a single test iteration (Entity and <see cref="TestCaseInfo"/>).
    /// </returns>
    public IEnumerable<object[]> GenerateCases()
    {
        foreach (var pipline in _piplines)
        {
            var generator = fakerFactory();

            foreach (var testCase in pipline(generator))
            {
                yield return testCase;
            }
        }
    }
}