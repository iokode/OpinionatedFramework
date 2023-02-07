using System;

namespace IOKode.OpinionatedFramework.Ensuring;

/// <summary>
/// Entry point for ensuring framework.
/// </summary>
#pragma warning disable CS1591
public static class Ensure
{
    public static ArgumentThrowerHolder Argument(string paramName, string? message = null) => new(paramName, message);
    public static ThrowerHolder Base() => new(new Exception());
    public static ThrowerHolder Base(string message) => new(new Exception(message));
    public static ThrowerHolder Exception(Exception ex) => new(ex);
    public static ThrowerHolder InvalidOperation(string message) => new(new InvalidOperationException(message));
}
#pragma warning restore CS1591

/// <summary>
/// A specified thrower for arguments.
/// It has a special validation that throws <see cref="ArgumentNullException"/> when object is null.
/// In all others validation, it throws <see cref="ArgumentException"/>.
/// </summary>
public class ArgumentThrowerHolder : ThrowerHolder
{
    internal ArgumentThrowerHolder(string paramName, string? message) : base(new ArgumentException(paramName, message))
    {
        if (string.IsNullOrWhiteSpace(paramName))
        {
            throw new ArgumentNullException(paramName);
        }
    }
    
    /// <summary>
    /// Ensure an object is not null.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when the object is null.</exception>
    public void NotNull(object? o)
    {
        if (o == null)
        {
            throw new ArgumentNullException();
        }
    }
}