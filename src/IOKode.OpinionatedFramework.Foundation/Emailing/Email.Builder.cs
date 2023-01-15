using System.Collections.Generic;
using IOKode.OpinionatedFramework.Ensuring;

namespace IOKode.OpinionatedFramework.Foundation.Emailing;

public partial record Email
{
    public class Builder
    {
        private EmailAddress? _from;
        private readonly HashSet<EmailAddress> _to = new();
        private readonly HashSet<EmailAddress> _carbonCopy = new();
        private readonly HashSet<EmailAddress> _blindCarbonCopy = new();
        private string? _subject;
        private string? _textContent;
        private string? _htmlContent;
        private readonly List<EmailAttachment> _attachments = new();

        public Builder From(EmailAddress address)
        {
            _from = address;

            return this;
        }

        public Builder To(params EmailAddress[] addresses)
        {
            foreach (var address in addresses)
            {
                _to.Add(address);
            }

            return this;
        }

        public Builder ReplaceTo(params EmailAddress[] addresses)
        {
            _to.Clear();
            To(addresses);

            return this;
        }

        public Builder Subject(string subject)
        {
            _subject = subject;

            return this;
        }

        public Builder TextContent(string content)
        {
            _textContent = content;

            return this;
        }

        public Builder HtmlContent(string content)
        {
            _htmlContent = content;

            return this;
        }

        public Builder Content(EmailContentBuilder builder)
        {
            TextContent(builder.ToText());
            HtmlContent(builder.ToHtml());

            return this;
        }

        public Email ToEmail()
        {
            Ensure.InvalidOperation("Cannot build an email because there are any \"to\".").Enumerable.NotEmpty(_to);
            Ensure.InvalidOperation("Cannot build an email because there are any \"from\".").Object.NotNull(_from);

            var email = new Email
            {
                From = _from,
                Subject = _subject,
                To = _to,
                HtmlContent = _htmlContent,
                TextContent = _textContent,
                Attachments = _attachments,
                CarbonCopy = _carbonCopy,
                BlindCarbonCopy = _blindCarbonCopy
            };

            return email;
        }
    }
}