using System;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Events;
using IOKode.OpinionatedFramework.Jobs;

namespace IOKode.OpinionatedFramework.ContractImplementations.Hangfire;

/// <summary>
/// Dispatches events by enqueuing one Hangfire job per handler.
/// </summary>
/// <remarks>
/// The event is durable once the job is stored, so handlers survive a restart of the dispatching process. The
/// enqueue happens outside any transaction the application may have open, so a crash between committing a
/// business change and enqueuing loses the handlers for that change.
/// </remarks>
/// <remarks>
/// Fan-out happens here, before anything is enqueued, which is why this driver requires every handler to be
/// declared in the dispatching process. A handler living in another process cannot be reached.
/// </remarks>
public class HangfireEventDispatcher(
    HangfireEventsOptions options,
    IJobEnqueuer jobEnqueuer,
    string queueName) : IEventDispatcher
{
    /// <inheritdoc/>
    public async Task DispatchAsync(IPublishableEvent @event, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(@event);

        // The payload carries the declared name and the job rebuilds the event from the map built out of the
        // declarations, so an event never declared would be stored under a name no job can read back.
        options.EnsureDeclaredAsPublished(@event.GetType());

        var payload = EventPayload.From(@event);
        var queue = Queue.Create(queueName);

        foreach (var registration in options.GetRegistrationsFor(@event.GetType()))
        {
            // The handler is identified by name rather than by System.Type, so a job already stored still
            // resolves after the assembly is renamed or its version changes.
            var creator = new ExecuteEventHandlerJobCreator(
                payload.Name,
                payload.Body,
                registration.HandlerType.FullName!);

            await jobEnqueuer.EnqueueAsync(queue, creator, cancellationToken);
        }
    }
}
