using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Facades;

namespace IOKode.OpinionatedFramework.Emailing;

[AddToFacade("Email")]
public interface IEmailSender
{
    /// <summary>
    /// Send an email.
    /// </summary>
    /// <exception cref="EmailException">Cannot send the email.</exception>
    public Task SendAsync(Email email, CancellationToken cancellationToken);
}