using FluentAssertions;
using FluentValidation;
using RewardFlow_UnitTest.Employees;
using System.Linq.Expressions;

namespace RewardFlow_UnitTest.Infurstructure;

public abstract class RequestValidatorTestBase<TEntity, TValidator, TRequest, TInput>
    where TEntity : class
    where TValidator : IValidator<TRequest>, new()
{
    protected TValidator Validator => new();
    protected abstract TRequest MapToRequest(TInput entity);
    
    protected async Task RunTest(TInput entity, TestCaseInfo testCaseInfo, bool shouldBeValid = true)
    {
        // Arrange
        var request = MapToRequest(entity);

        // Act
        var result = await Validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().Be(shouldBeValid, testCaseInfo.ToString());
    }
    
    public abstract Task InvalidRequest_ShouldReturnInvalid(TInput input, TestCaseInfo testCaseInfo);
    public abstract Task ValidRequest_ShouldReturnValid(TInput input, TestCaseInfo testCaseInfo);
}

public interface IValidatorDataProvider<TEntity>
{
    static abstract IEnumerable<object> GetTestData(bool shouldBeValid = true);
    static abstract IEnumerable<object> GetTestData<TProperty>
        (Expression<Func<TEntity,TProperty>> property,bool shouldBeValid = true);
}