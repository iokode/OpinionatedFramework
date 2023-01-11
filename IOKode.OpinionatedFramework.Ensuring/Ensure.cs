using System;
using IOKode.OpinionatedFramework.Ensuring.Throwers;

namespace IOKode.OpinionatedFramework.Ensuring;

public static class Ensure
{
    public static ArgumentEnsuringHold Argument(string paramName) => new ArgumentEnsuringHold();
    public static ThrowerHolder2 Operation(string message) => new ThrowerHolder2(new InvalidOperationException(message));
    public static ThrowerHolder2 Generic() => new ThrowerHolder2(new Exception());
    public static ThrowerHolder2 Exception(Exception ex) => new ThrowerHolder2(ex);
}

public class ThrowerHolder2
{
    // todo remove this class
    private readonly Exception _exception;

    public ThrowerHolder2(Exception exception)
    {
        _exception = exception;
    }

    public CollectionThrower Collection => new (_exception);
}

public class ArgumentEnsuringHold : ThrowerHolder2
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