namespace IOKode.OpinionatedFramework.Ensuring.Ensurers;

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