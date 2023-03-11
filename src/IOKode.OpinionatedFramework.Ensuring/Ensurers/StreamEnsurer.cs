using System;
using System.IO;

namespace IOKode.OpinionatedFramework.Ensuring.Ensurers;

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
        Ensure.Argument(nameof(stream)).NotNull(stream);
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
        Ensure.Argument(nameof(stream)).NotNull(stream);
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
        Ensure.Argument(nameof(stream)).NotNull(stream);
        return stream.CanSeek;
    }
}