using System.Collections.Generic;

namespace IOKode.OpinionatedFramework.Tests.MailKit;

public class MailHogEmailResponse
{
    public int Total { get; set; }
    public int Count { get; set; }
    public int Start { get; set; }
    public List<MailHogEmailItem> Items { get; set; } = [];
}

public class MailHogEmailItem
{
    public required string Id { get; set; }
    public required MailHogEmailFrom From { get; set; }
    public required List<MailHogEmailTo> To { get; set; }
    public required MailHogEmailContent Content { get; set; }
    public required MailHogEmailRaw Raw { get; set; }
}

public class MailHogEmailFrom
{
    public required string Mailbox { get; set; }
    public required string Domain { get; set; }
}

public class MailHogEmailTo
{
    public required string Mailbox { get; set; }
    public required string Domain { get; set; }
}

public class MailHogEmailContent
{
    public Dictionary<string, List<string>> Headers { get; set; } = [];
    public required string Body { get; set; }
}

public class MailHogEmailRaw
{
    public required string From { get; set; }
    public required List<string> To { get; set; }
}