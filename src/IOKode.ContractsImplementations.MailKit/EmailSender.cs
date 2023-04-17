using IOKode.OpinionatedFramework.Contracts;
using IOKode.OpinionatedFramework.Foundation.Emailing;
using MailKit.Net.Smtp;
using MimeKit;

public class EmailSender : IEmailSender
{
    private readonly SmtpClient _client;
    private readonly string _username;
    private readonly string _password;

    public EmailSender(string smtpHost, int smtpPort, string username, string password)
    {
        _client = new SmtpClient();
        _client.Connect(smtpHost, smtpPort);
        _username = username;
        _password = password;
    }

    public async Task SendAsync(Email email, CancellationToken cancellationToken)
    {
        try
        {
            await _client.AuthenticateAsync(_username, _password, cancellationToken);

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
}