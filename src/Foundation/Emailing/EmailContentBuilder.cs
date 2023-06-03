using System;
using System.Collections.Generic;
using System.Text;
using IOKode.OpinionatedFramework.Emailing.Elements;

namespace IOKode.OpinionatedFramework.Emailing;

public class EmailContentBuilder
{
    private readonly List<EmailElement> _contentElements = new();

    public EmailContentBuilder AddElement(EmailElement element)
    {
        _contentElements.Add(element);
        return this;
    }

    public string ToText()
    {
        var sb = new StringBuilder();

        foreach (var element in _contentElements)
        {
            sb.Append(element.ToText());
        }

        return sb.ToString();
    }

    public string ToHtml()
    {
        // Implement HTML rendering logic based on _contentElements and Stylesheet
        throw new NotImplementedException();
    }

    public Email.Builder ToEmailBuilder()
    {
        var builder = new Email.Builder();
        return builder.Content(this);
    }
}