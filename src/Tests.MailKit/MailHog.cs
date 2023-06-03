namespace IOKode.OpinionatedFramework.Tests.MailKit;

public class MailHogEmailResponse
{
    public int Total { get; set; }
    public int Count { get; set; }
    public int Start { get; set; }
    public List<MailHogEmailItem> Items { get; set; }
}

public class MailHogEmailItem
{
    public string ID { get; set; }
    public MailHogEmailFrom From { get; set; }
    public List<MailHogEmailTo> To { get; set; }
    public MailHogEmailContent Content { get; set; }
    public MailHogEmailRaw Raw { get; set; }
}

public class MailHogEmailFrom
{
    public string Mailbox { get; set; }
    public string Domain { get; set; }
}

public class MailHogEmailTo
{
    public string Mailbox { get; set; }
    public string Domain { get; set; }
}

public class MailHogEmailContent
{
    public Dictionary<string, List<string>> Headers { get; set; }
    public string Body { get; set; }
}

public class MailHogEmailRaw
{
    public string From { get; set; }
    public List<string> To { get; set; }
}