using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Foundation.Emailing;

namespace IOKode.OpinionatedFramework.Contracts;

public interface IEmailSender
{
    public Task SendAsync(Email email, CancellationToken cancellationToken);
}