using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Emailing;
using IOKode.OpinionatedFramework.Facades;
using Email = IOKode.OpinionatedFramework.Emailing.Email;

namespace IOKode.OpinionatedFramework.Contracts;

[AddToFacade("Email")]
public interface IEmailSender
{
    /// <summary>
    /// Send an email.
    /// </summary>
    /// <exception cref="EmailException">Cannot send the email.</exception>
    public Task SendAsync(Email email, CancellationToken cancellationToken);
}