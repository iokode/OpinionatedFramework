namespace IOKode.OpinionatedFramework.Ensuring.Ensurers;

/// <summary>
/// Provides methods for validating conditions related to booleans within the Ensure API.
/// Decorated with the EnsurerAttribute, it is a part of the Ensure validation mechanism.
/// </summary>
/// <remarks>
/// As part of the Ensure API mechanism, this class is static.
/// </remarks>
[Ensurer]
public static class BooleanEnsurer
{
    /// <summary>
    /// Check that the specified boolean is true.
    /// </summary>
    /// <param name="boolean">The condition to evaluate.</param>
    /// <returns>true if the specified boolean is true; otherwise, false.</returns>
    public static bool IsTrue(bool boolean)
    {
        return boolean;
    }

    /// <summary>
    /// Check that the specified boolean is false.
    /// </summary>
    /// <param name="boolean">The condition to evaluate.</param>
    /// <returns>true if the specified boolean is false; otherwise, false.</returns>
    public static bool IsFalse(bool boolean)
    {
        return !boolean;
    }
}