using System.IO;

namespace IOKode.OpinionatedFramework.Ensuring.Ensurers;

[Ensurer]
public static class StreamEnsurer
{
    public static bool CanRead(Stream stream)
    {
        Ensure.Argument(nameof(stream)).NotNull(stream);

        if (!stream.CanRead)
        {
            return false;
        }

        return true;
    }

    public static bool CanWrite(Stream stream)
    {
        Ensure.Argument(nameof(stream)).NotNull(stream);

        if (!stream.CanWrite)
        {
            return false;
        }

        return true;
    }

    public static bool CanSeek(Stream stream)
    {
        Ensure.Argument(nameof(stream)).NotNull(stream);

        if (!stream.CanSeek)
        {
            return false;
        }

        return true;
    }
}