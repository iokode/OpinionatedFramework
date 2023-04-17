using System.IO;
using System.Net.Mime;

namespace IOKode.OpinionatedFramework.Foundation.Emailing;

public record EmailAttachment
{
    public required string FileName { get; init; }
    public required Stream Content { get; init; }
    public ContentType? ContentType { get; init; }
}