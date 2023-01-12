namespace IOKode.OpinionatedFramework.Ensuring.Ensurers;

[Ensurer]
public static class AssertionEnsurer
{
    public static bool Assert(bool condition)
    {
        return condition;
    }
}