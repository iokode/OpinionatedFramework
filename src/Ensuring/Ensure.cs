using System;
using System.Runtime.CompilerServices;

namespace IOKode.OpinionatedFramework.Ensuring;

/// <summary>
/// Entry point to the Ensure API.
/// </summary>
public static partial class Ensure
{
    /// <summary>
    /// Ensures that the given argument object is not null.
    /// </summary>
    /// <param name="obj">The object to check for nullity.</param>
    /// <param name="argumentName">The name of the argument that is checked for nullity. This is automatically captured from the call site.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="obj"/> argument is null.</exception>
    public static void ArgumentNotNull(object? obj, [CallerArgumentExpression(nameof(obj))] string? argumentName = null) =>
        Ensure.Object.NotNull(obj).ElseThrowsNullArgument(argumentName!);
}