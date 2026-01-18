using Bogus;

namespace RewardFlow.IntegrationTests.Infrastructure.DataGenerators;

public class FakerRule<T> where T : class
{
    private Func<Faker,T> _defaultRule;
    private Func<Faker,T> _customRule;
    private float? _nullWeight;
        
    public FakerRule(Func<Faker, T> defaultRule, float? nullWeight = null)
    {
        _defaultRule = defaultRule;
        _nullWeight = nullWeight;
    }
        
    public FakerRule<T> SetCustomRule(Func<Faker, T> customRule)
    {
        _customRule = customRule;
        return this;
    }
        
    public FakerRule<T> SetNullWeight(float nullWeight)
    {
        _nullWeight = nullWeight;
        return this;
    }
        
    public Func<Faker,T?> GetRule()
    {
        if (_customRule != null) return _customRule;
        if (_nullWeight != null) return f => _defaultRule(f).OrNull(f, _nullWeight.Value);
        return _defaultRule;
    }
}