using System;
using IOKode.OpinionatedFramework.Ensuring.Ensurers;

namespace IOKode.OpinionatedFramework.Ensuring;

public static class Ensure
{
    public static ArgumentEnsuringHold Argument(string paramName) => new ArgumentEnsuringHold();
    public static EnsurerHold Operation(string message) => new EnsurerHold(new InvalidOperationException(message));
    public static EnsurerHold Generic() => new EnsurerHold(new Exception());
    public static EnsurerHold Exception(Exception ex) => new EnsurerHold(ex);
}

public class EnsurerHold
{
    private readonly Exception _exception;

    public EnsurerHold(Exception exception)
    {
        _exception = exception;
    }
    
    public GeneratedCollectionEnsurer Collection => new GeneratedCollectionEnsurer(_exception);
}

public class ArgumentEnsuringHold : EnsurerHold
{
    public ArgumentEnsuringHold() : base(new ArgumentException())
    {
        
    }
    
    public void NotNull(object? o)
    {
        if (o == null)
        {
            throw new ArgumentNullException();
        }
    }
}