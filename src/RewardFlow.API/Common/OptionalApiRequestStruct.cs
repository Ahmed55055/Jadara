namespace Reward_Flow_v2.Common;

public struct Optional<T>
{
    public static implicit operator Optional<T>(T value) => new(value);
    public static implicit operator T (Optional<T> value) => value.Value;

    public bool HasValue = false;
    public T Value;
    
    public Optional()
    { }
    
    public Optional(T value)
    {
        Value = value;
        HasValue = true;
    }
}