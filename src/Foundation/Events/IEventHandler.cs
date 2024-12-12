using System;
using System.Threading;
using System.Threading.Tasks;

namespace IOKode.OpinionatedFramework.Events;

public interface IEventHandler<TEvent> where TEvent : Event
{
    public Task HandleAsync(TEvent @event, CancellationToken cancellationToken);
}