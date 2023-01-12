using System;

namespace IOKode.OpinionatedFramework.Foundation.Emailing;

public class EmailContentBuilder
{
    public EmailContentBuilder Image(Uri logoUri)
    {
        throw new NotImplementedException();
    }

    public EmailContentBuilder Image(object image)
    {
        throw new NotImplementedException();
    }

    public EmailContentBuilder Header(string text)
    {
        throw new NotImplementedException();
    }

    public EmailContentBuilder Paragraph(string text)
    {
        throw new NotImplementedException();
    }

    public EmailContentBuilder Button(string text, Uri action)
    {
        throw new NotImplementedException();
    }

    public EmailContentBuilder TableWithTitleColumn(string[] titleRow, string[][] rows)
    {
        throw new NotImplementedException();
    }

    public EmailContentBuilder Table(string[][] rows)
    {
        throw new NotImplementedException();
    }

    public string ToText()
    {
        throw new NotImplementedException();
    }

    public string ToHtml()
    {
        throw new NotImplementedException();
    }

    public Email.Builder ToEmailBuilder()
    {
        var builder = new Email.Builder();
        return builder.Content(this);
    }
}