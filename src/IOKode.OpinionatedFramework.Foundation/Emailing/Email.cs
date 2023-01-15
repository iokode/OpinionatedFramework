using System.Collections.Generic;
using IOKode.OpinionatedFramework.Ensuring;

namespace IOKode.OpinionatedFramework.Foundation.Emailing;

public partial record Email
{
    private readonly ISet<EmailAddress> _to = new HashSet<EmailAddress>();

    public required EmailAddress From { get; init; }
    public required string? Subject { get; init; }

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

    public ISet<EmailAddress> CarbonCopy { get; init; } = new HashSet<EmailAddress>();
    public ISet<EmailAddress> BlindCarbonCopy { get; init; } = new HashSet<EmailAddress>();
    public string? TextContent { get; init; }
    public string? HtmlContent { get; init; }
    public IList<EmailAttachment> Attachments { get; init; } = new List<EmailAttachment>();

    private Email()
    {
    }

    public static Builder CreateBuilder()
    {
        var builder = new Builder();
        return builder;
    }
}