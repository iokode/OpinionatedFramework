using System;

namespace IOKode.OpinionatedFramework.Ensuring;

/// <summary>
/// Provides several extension methods to the <see cref="Thrower"/> class, each offering 
/// a convenient way to throw specific types of exceptions.
/// </summary>
public static class ThrowerExtensions
{
    /// <summary>
    /// Throws an <see cref="ArgumentNullException"/> if the validation did not pass.
    /// </summary>
    /// <param name="thrower">The Thrower instance representing the validation result.</param>
    /// <param name="argumentName">The name of the argument that causes the exception.</param>
    public static void ElseThrowsNullArgument(this Thrower thrower, string argumentName)
    {
        thrower.ElseThrows(new ArgumentNullException(argumentName));
    }

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> if the validation did not pass.
    /// </summary>
    /// <param name="thrower">The Thrower instance representing the validation result.</param>
    /// <param name="message">A message that describes the error.</param>
    /// <param name="argumentName">The name of the argument that causes the exception.</param>
    public static void ElseThrowsIllegalArgument(this Thrower thrower, string message, string argumentName)
    {
        thrower.ElseThrows(new ArgumentException(message, argumentName));
    }

    /// <summary>
    /// Throws an <see cref="InvalidOperationException"/> if the validation did not pass.
    /// </summary>
    /// <param name="thrower">The Thrower instance representing the validation result.</param>
    /// <param name="message">A message that describes the error.</param>
    public static void ElseThrowsInvalidOperation(this Thrower thrower, string message)
    {
        thrower.ElseThrows(new InvalidOperationException(message));
    }

    /// <summary>
    /// Throws a base Exception if the validation did not pass.
    /// </summary>
    /// <param name="thrower">The Thrower instance representing the validation result.</param>
    /// <param name="message">A message that describes the error.</param>
    public static void ElseThrowsBase(this Thrower thrower, string message)
    {
        thrower.ElseThrows(new Exception(message));
    }
}