namespace IOKode.OpinionatedFramework.Ensuring.Ensurers;

/// <summary>
/// Provides methods for validating conditions related to objects within the Ensure API.
/// Decorated with the EnsurerAttribute, it is a part of the Ensure validation mechanism.
/// </summary>
/// <remarks>
/// As part of the Ensure API mechanism, this class is static.
/// </remarks>
[Ensurer]
public static class ObjectEnsurer
{
    /// <summary>
    /// Validates that the given object is not null.
    /// </summary>
    /// <param name="value">The object to be validated.</param>
    /// <returns>A boolean value indicating whether the object is not null. Returns true if the object is not null; otherwise, false.</returns>
    public static bool NotNull(object? value)
    {
        return value != null;
    }
}