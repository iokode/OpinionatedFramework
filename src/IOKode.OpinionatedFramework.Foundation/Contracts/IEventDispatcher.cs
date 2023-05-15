using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Events;

namespace IOKode.OpinionatedFramework.Contracts;

public interface IEventDispatcher
{
    public Task DispatchAsync(Event @event, CancellationToken cancellationToken);
}