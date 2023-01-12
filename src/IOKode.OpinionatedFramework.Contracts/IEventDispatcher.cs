using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Foundation;
using IOKode.OpinionatedFramework.Foundation.Events;

namespace IOKode.OpinionatedFramework.Contracts;

public interface IEventDispatcher
{
    public Task DispatchAsync(Event @event, CancellationToken cancellationToken);
}