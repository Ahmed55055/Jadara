namespace RewardFlow_UnitTest.Employees;

public readonly struct TestCaseInfo(object? entity, string propertyName, object? value, string? description = null)
{
    public object? Entity { get; } = entity;
    public string? Description { get; } = description;
    public object? Value { get; } = value;
    public string PropertyName { get; } = propertyName;

    public override string ToString() => $"Prop: {PropertyName} | Value: [{Value}] | {Description}";
}