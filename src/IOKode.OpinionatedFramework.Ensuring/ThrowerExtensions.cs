using System;

namespace IOKode.OpinionatedFramework.Ensuring;

public static class ThrowerExtensions
{
    public static void ElseThrowsNullArgument(this Thrower thrower, string argumentName)
    {
        thrower.ElseThrows(new ArgumentNullException(argumentName));
    }

    public static void ElseThrowsIllegalArgument(this Thrower thrower, string message, string argumentName)
    {
        thrower.ElseThrows(new ArgumentException(message, argumentName));
    }

    public static void ElseThrowsInvalidOperation(this Thrower thrower, string message)
    {
        thrower.ElseThrows(new InvalidOperationException(message));
    }
    
    public static void ElseThrowsBase(this Thrower thrower, string message)
    {
        thrower.ElseThrows(new Exception(message));
    }
}