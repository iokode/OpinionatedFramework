using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Contracts;
using IOKode.OpinionatedFramework.Events;

namespace IOKode.OpinionatedFramework.Extensions;

public static class EventExtensions
{
    public static async Task DispatchAsync(this Event @event, CancellationToken cancellationToken = default)
    {
        var dispatcher = Locator.Resolve<IEventDispatcher>();
        await dispatcher.DispatchAsync(@event, cancellationToken);
    }
}