using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Foundation;

namespace IOKode.OpinionatedFramework.Contracts.Facades;

public static class Email
{
    public static async Task SendAsync(Foundation.Emailing.Email email, CancellationToken cancellationToken = default)
    {
        var sender = Locator.Resolve<IEmailSender>();
        await sender.SendAsync(email, cancellationToken);
    }
}