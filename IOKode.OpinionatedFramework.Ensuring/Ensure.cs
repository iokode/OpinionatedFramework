using System;
using IOKode.OpinionatedFramework.Ensuring.Ensurers;

namespace IOKode.OpinionatedFramework.Ensuring;

public static class Ensure
{
    public static ArgumentEnsuringHold Argument() => new ArgumentEnsuringHold();
    public static EnsurerHold Operation(string message) => new EnsurerHold();
    public static EnsurerHold Generic() => new EnsurerHold();
    public static EnsurerHold Exception(Exception ex) => new EnsurerHold();
}

public class EnsurerHold
{
    public StringEnsurer String => new StringEnsurer();
    public TypeEnsurer Type => new TypeEnsurer();
    public StreamEnsurer Stream => new StreamEnsurer();
    public CollectionEnsurer Collection => new CollectionEnsurer();
    public ObjectEnsurer Object => new ObjectEnsurer();
    public NumberEnsurer Number => new NumberEnsurer();
    public AssertionEnsurer Assert => new AssertionEnsurer();
}

public class ArgumentEnsuringHold : EnsurerHold
{
    public bool NotNull(object? o)
    {
        return o != null;
    }
}