using System.Threading;
using System.Threading.Tasks;

namespace IOKode.OpinionatedFramework.Events;

public static class EventExtensions
{
    public static async Task DispatchAsync(this Event @event, CancellationToken cancellationToken = default)
    {
        var dispatcher = Locator.Resolve<IEventDispatcher>();
        await dispatcher.DispatchAsync(@event, cancellationToken);
    }
}