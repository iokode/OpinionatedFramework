using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Emailing;
using IOKode.OpinionatedFramework.Facades;
using IOKode.OpinionatedFramework.Logging;
using Microsoft.Extensions.Logging;
using Email = IOKode.OpinionatedFramework.Emailing.Email;

namespace IOKode.OpinionatedFramework.ContractImplementations.LoggerEmail;

public class LoggerEmailSender(ILogging log) : IEmailSender
{
    public Task SendAsync(Email email, CancellationToken cancellationToken)
    {
        const string logTemplate = """
                                   New email:
                                     - From: {from}
                                     - To: {to} and {receiversMinusOne} more addresses
                                     - Subject: {subject}
                                     - Attachments: {attachmentCount}
                                   Content:
                                   {textContent}
                                   """;

        log.Info(logTemplate, email.From, email.To, email.To.Count - 1, email.Subject, email.Attachments.Count, email.TextContent);
        return Task.CompletedTask;
    }
}