using System;

namespace IOKode.OpinionatedFramework.Ensuring.Ensurers;

/// <summary>
/// Provides methods for validating conditions related to types within the Ensure API.
/// Decorated with the EnsurerAttribute, it is a part of the Ensure validation mechanism.
/// </summary>
/// <remarks>
/// As part of the Ensure API mechanism, this class is static.
/// </remarks>
[Ensurer]
public static class TypeEnsurer
{
    /// <summary>
    /// Validates that the provided type can be assigned to the expected type.
    /// </summary>
    /// <param name="value">The type instance to be validated.</param>
    /// <param name="expectedType">The expected type instance.</param>
    /// <returns>
    /// A boolean value indicating whether the provided type can be assigned to the expected type. 
    /// Returns true if it can be assigned; otherwise, false.
    /// </returns>
    public static bool IsAssignableTo(Type value, Type expectedType)
    {
        Ensure.ArgumentNotNull(value);
        Ensure.ArgumentNotNull(expectedType);

        return value.IsAssignableTo(expectedType);
    }

    /// <summary>
    /// Validates that the provided type is a reference type.
    /// </summary>
    /// <param name="value">The Type instance to be validated.</param>
    /// <returns>
    /// A boolean value indicating whether the provided type is a reference type. 
    /// Returns true if it is a reference type; otherwise, false.
    /// </returns>
    public static bool IsReferenceType(Type value)
    {
        return !IsValueType(value);
    }

    /// <summary>
    /// Validates that the provided type is a value type.
    /// </summary>
    /// <param name="value">The Type instance to be validated.</param>
    /// <returns>
    /// A boolean value indicating whether the provided type is a value type. 
    /// Returns true if it is a value type; otherwise, false.
    /// </returns>
    public static bool IsValueType(Type value)
    {
        Ensure.ArgumentNotNull(value);

        return value.IsValueType;
    }
}