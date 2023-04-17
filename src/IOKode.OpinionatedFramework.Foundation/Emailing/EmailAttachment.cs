using System.IO;
using System.Net.Mime;

namespace IOKode.OpinionatedFramework.Foundation.Emailing;

/// <summary>
/// Represents an email attachment to be sent with an email message.
/// </summary>
public record EmailAttachment
{
    /// <summary>
    /// Gets the file name of the attachment, including its file extension.
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// Gets the content stream of the attachment.
    /// </summary>
    public required Stream Content { get; init; }

    /// <summary>
    /// Gets or sets the content type (MIME type) of the attachment.
    /// </summary>
    public ContentType? ContentType { get; init; }
}