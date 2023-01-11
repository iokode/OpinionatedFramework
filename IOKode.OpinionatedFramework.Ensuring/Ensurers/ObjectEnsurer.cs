namespace IOKode.OpinionatedFramework.Ensuring.Ensurers;

[Ensurer]
public static class ObjectEnsurer
{
    public static bool NotNull(object? value)
    {
        return value != null;
    }
}