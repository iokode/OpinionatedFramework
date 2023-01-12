namespace IOKode.OpinionatedFramework.Ensuring.Ensurers;

[Ensurer]
public static class TypeEnsurer
{
    public static bool IsAssignableTo(System.Type value, System.Type expectedType)
    {
        Ensure.Argument(nameof(value)).NotNull(value);
        Ensure.Argument(nameof(value)).NotNull(expectedType);

        return value.IsAssignableTo(expectedType);
    }
}