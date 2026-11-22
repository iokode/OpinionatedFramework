using System;

namespace IOKode.OpinionatedFramework.Commands;

/// <summary>
/// An error found while validating the data of a command or an input.
/// </summary>
public abstract class ValidationError
{
    private protected ValidationError(string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        Message = message;
    }

    /// <summary>
    /// What is wrong with the data.
    /// </summary>
    public string Message { get; }
}

/// <summary>
/// A validation error about the data at a path of the command or input that was validated.
/// </summary>
public sealed class PathValidationError : ValidationError
{
    /// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="message"/> is <see langword="null"/>, empty or white space.</exception>
    public PathValidationError(ValidationPath path, string message) : base(message)
    {
        ArgumentNullException.ThrowIfNull(path);
        Path = path;
    }

    /// <summary>
    /// The path of the data the error is about.
    /// </summary>
    public ValidationPath Path { get; }

    public override string ToString()
    {
        return $"{Path}: {Message}";
    }
}

/// <summary>
/// A validation error about the object that was validated as a whole, such as a rule that involves several of its
/// fields.
/// </summary>
/// <remarks>
/// When an input reports it, it reaches the command as a <see cref="PathValidationError"/> with the path of that input.
/// When the command reports it, it stays an error about the whole command.
/// </remarks>
public sealed class ObjectValidationError : ValidationError
{
    /// <exception cref="ArgumentException"><paramref name="message"/> is <see langword="null"/>, empty or white space.</exception>
    public ObjectValidationError(string message) : base(message)
    {
    }

    public override string ToString()
    {
        return Message;
    }
}
