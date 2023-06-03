using System;
using System.IO;

namespace IOKode.OpinionatedFramework.Ensuring.Ensurers;

/// <summary>
/// Provides methods for validating conditions related to streams within the Ensure API.
/// Decorated with the EnsurerAttribute, it is a part of the Ensure validation mechanism.
/// </summary>
/// <remarks>
/// As part of the Ensure API mechanism, this class is static.
/// </remarks>
[Ensurer]
public static class StreamEnsurer
{
    /// <summary>
    /// Determines whether the specified stream can be read from.
    /// </summary>
    /// <param name="stream">The stream to check.</param>
    /// <returns>true if the specified stream can be read from; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the input stream is null.</exception>
    public static bool CanRead(Stream stream)
    {
        Ensure.ArgumentNotNull(stream);
        return stream.CanRead;
    }

    /// <summary>
    /// Determines whether the specified stream can be written to.
    /// </summary>
    /// <param name="stream">The stream to check.</param>
    /// <returns>true if the specified stream can be written to; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the input stream is null.</exception>
    public static bool CanWrite(Stream stream)
    {
        Ensure.ArgumentNotNull(stream);
        return stream.CanWrite;
    }

    /// <summary>
    /// Determines whether the specified stream supports seeking.
    /// </summary>
    /// <param name="stream">The stream to check.</param>
    /// <returns>true if the specified stream supports seeking; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the input stream is null.</exception>
    public static bool CanSeek(Stream stream)
    {
        Ensure.ArgumentNotNull(stream);
        return stream.CanSeek;
    }
}