namespace IOKode.OpinionatedFramework.Ensuring.Ensurers;

[Ensurer]
public class TypeEnsurer
{
    public bool IsAssignableTo(System.Type value, System.Type expectedType)
    {
        Ensure.Argument().NotNull(value);
        Ensure.Argument().NotNull(expectedType);

        return value.IsAssignableTo(expectedType);
    }
}