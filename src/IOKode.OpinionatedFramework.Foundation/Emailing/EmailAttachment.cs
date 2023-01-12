using System.IO;

namespace IOKode.OpinionatedFramework.Foundation.Emailing;

public record EmailAttachment
{
    public required string FileName { get; init; }
    public required Stream Content { get; init; }
}