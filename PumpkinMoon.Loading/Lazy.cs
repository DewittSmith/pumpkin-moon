using System;

namespace PumpkinMoon.Loading;

public class Lazy
{
    private readonly Lazy<object> innerValue;

    public Lazy(Lazy<object> innerValue)
    {
        this.innerValue = innerValue;
    }

    public Lazy(Func<object> valueFactory) : this(new Lazy<object>(valueFactory))
    {
    }

    public T GetValue<T>()
    {
        return (T)innerValue.Value;
    }

    public static implicit operator Lazy(Lazy<object> lazy)
    {
        return new Lazy(lazy);
    }

    public static implicit operator Lazy<object>(Lazy lazy)
    {
        return lazy.innerValue;
    }
}