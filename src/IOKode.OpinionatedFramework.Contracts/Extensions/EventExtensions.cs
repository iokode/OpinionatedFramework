using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Foundation;
using IOKode.OpinionatedFramework.Foundation.Events;

namespace IOKode.OpinionatedFramework.Contracts.Extensions;

public static class EventExtensions
{
    public static async Task DispatchAsync(this Event @event, CancellationToken cancellationToken = default)
    {
        var dispatcher = Locator.Resolve<IEventDispatcher>();
        await dispatcher.DispatchAsync(@event, cancellationToken);
    }
}