using System;
using System.Linq;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.ContractImplementations.Hangfire.Exceptions;
using IOKode.OpinionatedFramework.Jobs;
using IOKode.OpinionatedFramework.ServiceLocation;
using Microsoft.Extensions.DependencyInjection;

namespace IOKode.OpinionatedFramework.ContractImplementations.Hangfire;

/// <summary>
/// Runs one event handler for one dispatched event.
/// </summary>
/// <remarks>
/// The job carries the event itself rather than a reference to a stored row, so running it needs nothing but
/// the queue: there is no database the handler has to read the event back from.
/// </remarks>
/// <remarks>
/// A failure is allowed to leave this method so that Hangfire sees it, elects the failed state and applies the
/// retry policy the filter provider supplies for this handler. Catching it here would hide the attempt from the
/// dashboard.
/// </remarks>
/// <param name="eventName">The stable name of the dispatched event.</param>
/// <param name="eventBody">The event serialized as JSON.</param>
/// <param name="handlerTypeName">The full name of the handler type to run.</param>
public class ExecuteEventHandlerJob(string eventName, string eventBody, string handlerTypeName) : Job
{
    /// <inheritdoc/>
    public override async Task ExecuteAsync(IJobExecutionContext context)
    {
        // The container not being initialized is a state in which this call cannot mean anything, which is
        // what InvalidOperationException is for.
        var serviceProvider = Locator.ServiceProvider
            ?? throw new InvalidOperationException("The service container is not initialized.");

        var options = serviceProvider.GetRequiredService<HangfireEventsOptions>();
        var typeMap = serviceProvider.GetRequiredService<HangfireEventTypeMap>();

        var @event = new EventPayload(eventName, eventBody).ToEvent(typeMap.Value);

        var registration = options.GetRegistrationsFor(@event.GetType())
            .SingleOrDefault(candidate => candidate.HandlerType.FullName == handlerTypeName)
            ?? throw new UnknownEventHandlerException(handlerTypeName, eventName);

        await registration.Invoke(serviceProvider, @event, context.CancellationToken);
    }
}
