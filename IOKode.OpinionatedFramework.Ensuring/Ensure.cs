using System;
using IOKode.OpinionatedFramework.Ensuring.Throwers;

namespace IOKode.OpinionatedFramework.Ensuring;

public static class Ensure
{
    public static ArgumentEnsuringHold Argument(string paramName) => new ArgumentEnsuringHold();
    public static ThrowerHolder Operation(string message) => new ThrowerHolder(new InvalidOperationException(message));
    public static ThrowerHolder Generic() => new ThrowerHolder(new Exception());
    public static ThrowerHolder Exception(Exception ex) => new ThrowerHolder(ex);
}

public class ArgumentEnsuringHold : ThrowerHolder
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