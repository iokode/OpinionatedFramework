using IOKode.OpinionatedFramework.Contracts;
using IOKode.OpinionatedFramework.Foundation.Emailing;
using MailKit.Net.Smtp;
using MimeKit;

namespace IOKode.OpinionatedFramework.ContractImplementations.MailKit;

internal class MailKitEmailSender : IEmailSender, IDisposable
{
    private readonly MailKitOptions _options;
    private readonly SmtpClient _client;

    public MailKitEmailSender(MailKitOptions options)
    {
        _options = options;
        _client = new SmtpClient();
    }

    public async Task SendAsync(Email email, CancellationToken cancellationToken)
    {
        try
        {
            await _connectAsync(cancellationToken);

            if (_options.Authenticate)
            {
                await _client.AuthenticateAsync(_options.Username, _options.Password, cancellationToken);
            }

            var message = new MimeMessage();
            var bodyBuilder = new BodyBuilder();

            message.Subject = email.Subject;
            message.MessageId = email.MessageId.ToString();
            message.From.Add(MailboxAddress.Parse(email.From.ToString()));
            message.To.AddRange(email.To.Select(to => MailboxAddress.Parse(to.ToString())));

            if (email.ReplyTo != null)
            {
                message.ReplyTo.Add(MailboxAddress.Parse(email.ReplyTo.ToString()));
            }

            if (email.TextContent != null)
            {
                bodyBuilder.TextBody = email.TextContent;
            }

            if (email.HtmlContent != null)
            {
                bodyBuilder.HtmlBody = email.HtmlContent;
            }

            foreach (var attachment in email.Attachments)
            {
                await bodyBuilder.Attachments.AddAsync(attachment.FileName, attachment.Content, cancellationToken);
            }

            message.Body = bodyBuilder.ToMessageBody();

            await _client.SendAsync(message, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new EmailException(ex);
        }
        finally
        {
            await _client.DisconnectAsync(true, cancellationToken);
        }
    }

    public void Dispose()
    {
        _client.Dispose();
    }

    private async Task _connectAsync(CancellationToken cancellationToken)
    {
        if (_client.IsConnected)
        {
            return;
        }
        await _client.ConnectAsync(_options.Host, _options.Port, _options.Secure, cancellationToken);
    }
}