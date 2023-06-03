using System.Collections.Generic;
using IOKode.OpinionatedFramework.Ensuring;

namespace IOKode.OpinionatedFramework.Emailing;

public partial record Email
{
    /// <summary>
    /// A builder for creating instances of the <see cref="Email"/> class.
    /// </summary>
    public class Builder
    {
        private EmailAddress? _from;
        private MessageId? _messageId;
        private readonly HashSet<EmailAddress> _to = new();
        private readonly HashSet<EmailAddress> _carbonCopy = new();
        private readonly HashSet<EmailAddress> _blindCarbonCopy = new();
        private string? _subject;
        private string? _textContent;
        private string? _htmlContent;
        private readonly List<EmailAttachment> _attachments = new();

        /// <summary>
        /// Sets the unique identifier for the email message.
        /// </summary>
        /// <param name="messageId">The unique identifier to set.</param>
        /// <returns>The current <see cref="Builder"/> instance, allowing further configuration.</returns>
        public Builder MessageId(MessageId messageId)
        {
            _messageId = messageId;

            return this;
        }

        /// <summary>
        /// Sets the email address of the sender.
        /// </summary>
        /// <param name="address">The sender's email address.</param>
        /// <returns>The current <see cref="Builder"/> instance, allowing further configuration.</returns>
        public Builder From(EmailAddress address)
        {
            _from = address;

            return this;
        }

        /// <summary>
        /// Adds one or more email addresses to the recipients list.
        /// </summary>
        /// <param name="addresses">The email addresses to add.</param>
        /// <returns>The current <see cref="Builder"/> instance, allowing further configuration.</returns>
        public Builder To(params EmailAddress[] addresses)
        {
            foreach (var address in addresses)
            {
                _to.Add(address);
            }

            return this;
        }

        /// <summary>
        /// Replaces the current recipients list with the specified email addresses.
        /// </summary>
        /// <param name="addresses">The email addresses to set.</param>
        /// <returns>The current <see cref="Builder"/> instance, allowing further configuration.</returns>
        public Builder ReplaceTo(params EmailAddress[] addresses)
        {
            _to.Clear();
            To(addresses);

            return this;
        }

        /// <summary>
        /// Adds the specified email addresses to the carbon copy (CC) recipients list.
        /// </summary>
        /// <param name="addresses">An array of <see cref="EmailAddress"/> instances representing the email addresses to add to the carbon copy recipients list.</param>
        /// <returns>The current <see cref="Builder"/> instance, allowing further configuration.</returns>
        public Builder Cc(params EmailAddress[] addresses)
        {
            foreach (var address in addresses)
            {
                _carbonCopy.Add(address);
            }

            return this;
        }

        /// <summary>
        /// Replaces the current carbon copy (CC) recipients list with the specified email addresses.
        /// </summary>
        /// <param name="addresses">The email addresses to set.</param>
        /// <returns>The current <see cref="Builder"/> instance, allowing further configuration.</returns>
        public Builder ReplaceCc(params EmailAddress[] addresses)
        {
            _carbonCopy.Clear();
            Cc(addresses);

            return this;
        }

        /// <summary>
        /// Adds the specified email addresses to the blind carbon copy (BCC) recipients list.
        /// </summary>
        /// <param name="addresses">An array of <see cref="EmailAddress"/> instances representing the email addresses to add to the blind carbon copy recipients list.</param>
        /// <returns>The current <see cref="Builder"/> instance, allowing further configuration.</returns>
        public Builder Bcc(params EmailAddress[] addresses)
        {
            foreach (var address in addresses)
            {
                _blindCarbonCopy.Add(address);
            }

            return this;
        }

        /// <summary>
        /// Replaces the current blind carbon copy (BCC) recipients list with the specified email addresses.
        /// </summary>
        /// <param name="addresses">The email addresses to set.</param>
        /// <returns>The current <see cref="Builder"/> instance, allowing further configuration.</returns>
        public Builder ReplaceBcc(params EmailAddress[] addresses)
        {
            _blindCarbonCopy.Clear();
            Bcc(addresses);

            return this;
        }

        /// <summary>
        /// Sets the subject of the email.
        /// </summary>
        /// <param name="subject">The email subject.</param>
        /// <returns>The current <see cref="Builder"/> instance, allowing further configuration.</returns>
        public Builder Subject(string subject)
        {
            _subject = subject;

            return this;
        }

        /// <summary>
        /// Sets the text/plain content of the email.
        /// </summary>
        /// <param name="content">The text/plain content.</param>
        /// <returns>The current <see cref="Builder"/> instance, allowing further configuration.</returns>
        public Builder TextContent(string content)
        {
            _textContent = content;

            return this;
        }

        /// <summary>
        /// Sets the text/html content of the email.
        /// </summary>
        /// <param name="content">The text/html content.</param>
        /// <returns>The current <see cref="Builder"/> instance, allowing further configuration.</returns>
        public Builder HtmlContent(string content)
        {
            _htmlContent = content;

            return this;
        }

        /// <summary>
        /// Sets the content of the email using an <see cref="EmailContentBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="EmailContentBuilder"/> instance to use.</param>
        /// <returns>The current <see cref="Builder"/> instance, allowing further configuration.</returns>
        public Builder Content(EmailContentBuilder builder)
        {
            TextContent(builder.ToText());
            HtmlContent(builder.ToHtml());

            return this;
        }

        /// <summary>
        /// Adds the specified email attachments to the attachments list.
        /// </summary>
        /// <param name="attachments">An array of <see cref="EmailAttachment"/> instances representing the email attachments to add to the attachments list.</param>
        /// <returns>The current <see cref="Builder"/> instance, allowing further configuration.</returns>
        public Builder Attachment(params EmailAttachment[] attachments)
        {
            _attachments.AddRange(attachments);

            return this;
        }

        /// <summary>
        /// Replaces the current attachments list with the specified email attachments.
        /// </summary>
        /// <param name="attachments">An array of <see cref="EmailAttachment"/> instances representing the email attachments to replace the current attachments list with.</param>
        /// <returns>The current <see cref="Builder"/> instance, allowing further configuration.</returns>
        public Builder ReplaceAttachment(params EmailAttachment[] attachments)
        {
            _attachments.Clear();
            Attachment(attachments);

            return this;
        }

        /// <summary>
        /// Builds an instance of the <see cref="Email"/> class using the specified properties.
        /// </summary>
        /// <returns>An instance of the <see cref="Email"/> class with the specified properties.</returns>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown when the recipients list (To) is empty or when the sender's email address (From) is not set.
        /// </exception>
        public Email ToEmail()
        {
            Ensure.Enumerable.NotEmpty(_to).ElseThrowsInvalidOperation("Cannot build an email because there aren't any \"to\".");
            Ensure.Object.NotNull(_from).ElseThrowsInvalidOperation("Cannot build an email because there aren't any \"from\".");

            _messageId ??= Emailing.MessageId.GenerateMessageId(_from!.Host);

            var email = new Email
            {
                From = _from!,
                Subject = _subject,
                To = _to,
                HtmlContent = _htmlContent,
                TextContent = _textContent,
                Attachments = _attachments,
                CarbonCopy = _carbonCopy,
                BlindCarbonCopy = _blindCarbonCopy,
                MessageId = _messageId
            };

            return email;
        }
    }
}