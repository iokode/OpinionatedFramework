using System.Threading;
using System.Threading.Tasks;

namespace IOKode.OpinionatedFramework.Events;

public interface IEventDispatcher
{
    public Task DispatchAsync(Event @event, CancellationToken cancellationToken);
}