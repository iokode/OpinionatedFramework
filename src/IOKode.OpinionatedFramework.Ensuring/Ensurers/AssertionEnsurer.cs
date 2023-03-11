namespace IOKode.OpinionatedFramework.Ensuring.Ensurers;

[Ensurer]
public static class AssertionEnsurer
{
    /// <summary>
    /// Asserts that the specified condition is true.
    /// </summary>
    /// <param name="condition">The condition to evaluate.</param>
    /// <returns>true if the specified condition is true; otherwise, false.</returns>
    public static bool AssertTrue(bool condition)
    {
        return condition;
    }

    /// <summary>
    /// Asserts that the specified condition is false.
    /// </summary>
    /// <param name="condition">The condition to evaluate.</param>
    /// <returns>true if the specified condition is false; otherwise, false.</returns>
    public static bool AssertFalse(bool condition)
    {
        return !condition;
    }
}