using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Foundation.Emailing;

namespace IOKode.OpinionatedFramework.Contracts;

[AddToFacade("Email")]
public interface IEmailSender
{
    public Task SendAsync(Email email, CancellationToken cancellationToken);
}