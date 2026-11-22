using System;

namespace IOKode.OpinionatedFramework.Commands;

/// <summary>
/// The exception that is thrown when a dictionary of inputs holds a <see langword="null"/> key, which cannot be part of
/// the path of the errors of its value.
/// </summary>
public class NullDictionaryKeyException : Exception
{
    /// <param name="dictionaryType">The type of the dictionary that holds the <see langword="null"/> key.</param>
    /// <exception cref="ArgumentNullException"><paramref name="dictionaryType"/> is <see langword="null"/>.</exception>
    public NullDictionaryKeyException(Type dictionaryType) : base(BuildMessage(dictionaryType))
    {
        DictionaryType = dictionaryType;
    }

    /// <summary>
    /// The type of the dictionary that holds the <see langword="null"/> key.
    /// </summary>
    public Type DictionaryType { get; }

    private static string BuildMessage(Type dictionaryType)
    {
        ArgumentNullException.ThrowIfNull(dictionaryType);
        return $"The dictionary of inputs '{dictionaryType}' holds a null key; a key is part of the path of the errors of its value and cannot be null.";
    }
}
