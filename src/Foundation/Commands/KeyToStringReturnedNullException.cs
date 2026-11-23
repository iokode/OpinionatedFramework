using System;

namespace IOKode.OpinionatedFramework.Commands;

/// <summary>
/// The exception that is thrown when the <c>ToString</c> of a key of a dictionary of inputs returns
/// <see langword="null"/>, which breaks the contract of <see cref="object.ToString"/>: the text of the key is part of
/// the path of the errors of its value.
/// </summary>
public class KeyToStringReturnedNullException : Exception
{
    /// <param name="keyType">The type of the key whose <c>ToString</c> returned <see langword="null"/>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="keyType"/> is <see langword="null"/>.</exception>
    public KeyToStringReturnedNullException(Type keyType) : base(BuildMessage(keyType))
    {
        KeyType = keyType;
    }

    /// <summary>
    /// The type of the key whose <c>ToString</c> returned <see langword="null"/>.
    /// </summary>
    public Type KeyType { get; }

    private static string BuildMessage(Type keyType)
    {
        ArgumentNullException.ThrowIfNull(keyType);
        return $"The ToString of the key type '{keyType}' returned null; the text of a key is part of the path of the errors of its value.";
    }
}
