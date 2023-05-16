using System.Collections.Generic;
using IOKode.OpinionatedFramework.Ensuring;

namespace IOKode.OpinionatedFramework.Emailing;

/// <summary>
/// Represents an email message that may be sent or not. This class is designed to be used with
/// the IEmailSender contract. It contains all required data to send an email.
/// </summary>
/// <remarks>
/// This class is not meant to represent a sent email but only an email that is prepared to be sent.
/// It does not include information about sent date or message conversations and threads.
/// </remarks>
public partial record Email
{
    private readonly ISet<EmailAddress> _to = new HashSet<EmailAddress>();

    // To ensure email invariants, all instances should be created using Email.Builder class.
    private Email()
    {
    }

    /// <summary>
    /// Gets the email address of the sender.
    /// </summary>
    public required EmailAddress From { get; init; }

    /// <summary>
    /// Gets the subject of the email.
    /// </summary>
    public required string? Subject { get; init; }

    /// <summary>
    /// Gets the set of email addresses to which the email will be sent.
    /// </summary>
    public required ISet<EmailAddress> To
    {
        get => _to;
        init
        {
            Ensure.Argument(nameof(value)).NotNull(value);
            Ensure.Argument(nameof(value)).Enumerable.NotEmpty(value);
            _to = value;
        }
    }

    /// <summary>
    /// Gets the email address to which replies should be directed.
    /// </summary>
    public EmailAddress? ReplyTo { get; init; }

    /// <summary>
    /// Gets the unique identifier for this email message.
    /// </summary>
    public MessageId MessageId { get; init; } = null!;

    /// <summary>
    /// Gets the set of email addresses that will receive a carbon copy (CC) of the email.
    /// </summary>
    public ISet<EmailAddress> CarbonCopy { get; init; } = new HashSet<EmailAddress>();

    /// <summary>
    /// Gets the set of email addresses that will receive a blind carbon copy (BCC) of the email.
    /// </summary>
    public ISet<EmailAddress> BlindCarbonCopy { get; init; } = new HashSet<EmailAddress>();

    /// <summary>
    /// Gets the text/plain content of the email.
    /// </summary>
    public string? TextContent { get; init; }

    /// <summary>
    /// Gets the text/html content of the email.
    /// </summary>
    public string? HtmlContent { get; init; }

    /// <summary>
    /// Gets the list of attachments included with the email.
    /// </summary>
    public IList<EmailAttachment> Attachments { get; init; } = new List<EmailAttachment>();

    /// <summary>
    /// Creates a new instance of the <see cref="Email.Builder"/> class.
    /// </summary>
    /// <returns>A new instance of the <see cref="Email.Builder"/> class.</returns>
    public static Builder CreateBuilder()
    {
        var builder = new Builder();
        return builder;
    }
}