using System.Threading;
using System.Threading.Tasks;

namespace IOKode.OpinionatedFramework.Events;

public interface IEventHandler<TEvent> where TEvent : ISubscribableEvent
{
    public Task HandleAsync(TEvent @event, CancellationToken cancellationToken);
}
