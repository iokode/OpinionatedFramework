using System;

namespace IOKode.OpinionatedFramework.Configuration.Exceptions;

public class TypeMismatchException : Exception
{
    private readonly Type targetType;
    private readonly Type? foundType;

    public Type TargetType => this.targetType;
    public Type? FoundType => this.foundType;

    public TypeMismatchException(Type targetType, Type? foundType) : base($"Failed to convert value to type '{targetType.FullName}'. Found type: '{foundType?.FullName ?? "unknown"}'.")
    {
        this.targetType = targetType;
        this.foundType = foundType;
    }

    public TypeMismatchException(Type targetType, Type? foundType, Exception inner) : base($"Failed to convert value to type '{targetType.FullName}'. Found type: '{foundType?.FullName ?? "unknown"}'.", inner)
    {
        this.targetType = targetType;
        this.foundType = foundType;
    }
}