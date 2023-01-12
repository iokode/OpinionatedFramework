using System.Collections.Generic;
using IOKode.OpinionatedFramework.Ensuring;

namespace IOKode.OpinionatedFramework.Foundation.Emailing;

public partial record Email
{
    public class Builder
    {
        private EmailAddress _from;
        private readonly HashSet<EmailAddress> _to = new();
        private readonly HashSet<EmailAddress> _carbonCopy = new();
        private readonly HashSet<EmailAddress> _blindCarbonCopy = new();
        private string _subject;
        private string _textContent;
        private string _htmlContent;
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
            _ensure();

            var email = new Email
            {
                From = null,
                Subject = null,
                To = null
            };

            return email;
        }

        private void _ensure()
        {
            Ensure.Generic().Collection.NotEmpty(_to);
            Ensure.Generic().Object.NotNull(_from);
            Ensure.Generic().String.NotWhiteSpace(_subject);
        }
    }
}