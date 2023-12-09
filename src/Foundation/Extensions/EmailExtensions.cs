using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Emailing;
using IOKode.OpinionatedFramework.Jobs;

namespace IOKode.OpinionatedFramework.Extensions;

public static class EmailExtensions
{
    /// <summary>
    /// Send the email.
    /// </summary>
    /// <exception cref="EmailException">Something fail while trying to send the email.</exception>
    public static async Task SendAsync(this Email email, CancellationToken cancellationToken = default)
    {
        await Facades.Email.SendAsync(email, cancellationToken);
    }

    /// <summary>
    /// Enqueue the email.
    /// </summary>
    public static async Task QueueAsync(this Email email, Queue queue, CancellationToken cancellationToken = default)
    {
        var enqueuer = Locator.Resolve<IJobEnqueuer>();
        await enqueuer.EnqueueAsync(queue, new AsyncDelegateJob(async () => await email.SendAsync()), cancellationToken);
    }
}