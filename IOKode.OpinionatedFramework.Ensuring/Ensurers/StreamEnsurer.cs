using System.IO;

namespace IOKode.OpinionatedFramework.Ensuring.Ensurers;

[Ensurer]
public class StreamEnsurer
{
    public bool CanRead(Stream stream)
    {
        Ensure.Argument().NotNull(stream);

        if (!stream.CanRead)
        {
            return false;
        }

        return true;
    }

    public bool CanWrite(Stream stream)
    {
        Ensure.Argument().NotNull(stream);

        if (!stream.CanWrite)
        {
            return false;
        }

        return true;
    }

    public bool CanSeek(Stream stream)
    {
        Ensure.Argument().NotNull(stream);

        if (!stream.CanSeek)
        {
            return false;
        }

        return true;
    }
}