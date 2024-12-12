using System;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Events;
using IOKode.OpinionatedFramework.Facades;

namespace IOKode.OpinionatedFramework.ContractImplementations.Events;

public class EventDispatcher : IEventDispatcher
{
    public Task DispatchAsync(Event @event, CancellationToken cancellationToken)
    {
        var job = new DispatchEventJob(@event);
        Job.EnqueueAsync(job, cancellationToken);

        typeof(Event).GetProperty(nameof(Event.DispatchedAt))!.SetValue(@event, DateTimeOffset.UtcNow);
    }
}