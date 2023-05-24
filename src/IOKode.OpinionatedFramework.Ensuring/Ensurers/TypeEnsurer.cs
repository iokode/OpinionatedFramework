using System;

namespace IOKode.OpinionatedFramework.Ensuring.Ensurers;

[Ensurer]
public static class TypeEnsurer
{
    public static bool IsAssignableTo(Type value, Type expectedType)
    {
        Ensure.ArgumentNotNull(value);
        Ensure.ArgumentNotNull(expectedType);

        return value.IsAssignableTo(expectedType);
    }

    public static bool IsReferenceType(Type value)
    {
        return !IsValueType(value);
    }

    public static bool IsValueType(Type value)
    {
        Ensure.ArgumentNotNull(value);

        return value.IsValueType;
    }
}