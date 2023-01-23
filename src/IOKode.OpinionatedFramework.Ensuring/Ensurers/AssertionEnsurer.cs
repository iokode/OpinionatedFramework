namespace IOKode.OpinionatedFramework.Ensuring.Ensurers;

[Ensurer]
public static class AssertionEnsurer
{
    /// <summary>
    /// Ensure the condition is true.
    /// </summary>
    public static bool AssertTrue(bool condition)
    {
        return condition;
    }

    /// <summary>
    /// Ensure the condition is false.
    /// </summary>
    public static bool AssertFalse(bool condition)
    {
        return !condition;
    }
}