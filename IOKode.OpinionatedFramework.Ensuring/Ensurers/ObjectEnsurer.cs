namespace IOKode.OpinionatedFramework.Ensuring.Ensurers;

[Ensurer]
public class ObjectEnsurer
{
    public bool NotNull(object? value)
    {
        return value != null;
    }
}