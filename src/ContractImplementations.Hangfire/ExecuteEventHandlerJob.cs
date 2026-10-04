using System;
using System.Linq;
using System.Threading.Tasks;
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

/// <summary>
/// Creates an <see cref="ExecuteEventHandlerJob"/> from the values stored with the job.
/// </summary>
/// <remarks>
/// Every member is a string, so the creator serializes with any job storage and stays readable in the Hangfire
/// dashboard.
/// </remarks>
/// <param name="EventName">The stable name of the dispatched event.</param>
/// <param name="EventBody">The event serialized as JSON.</param>
/// <param name="HandlerTypeName">The full name of the handler type to run.</param>
public record ExecuteEventHandlerJobCreator(string EventName, string EventBody, string HandlerTypeName)
    : JobCreator<ExecuteEventHandlerJob>
{
    /// <inheritdoc/>
    public override ExecuteEventHandlerJob CreateJob() => new(EventName, EventBody, HandlerTypeName);

    /// <inheritdoc/>
    public override string GetJobName() => $"Handle {EventName} with {HandlerTypeName}";
}

/// <summary>
/// Thrown when a job names a handler that is no longer registered for the event it carries.
/// </summary>
/// <remarks>
/// A job outlives the process that enqueued it, so a handler removed or renamed while jobs were waiting leaves
/// them naming something this process does not know. The job fails with both names, which is what identifies
/// the stale job in the dashboard.
/// </remarks>
/// <param name="handlerTypeName">The handler type name stored with the job.</param>
/// <param name="eventName">The declared name of the event the job carries.</param>
public sealed class UnknownEventHandlerException(string handlerTypeName, string eventName)
    : Exception($"No handler named '{handlerTypeName}' is registered for event '{eventName}'.")
{
    /// <summary>Gets the handler type name stored with the job.</summary>
    public string HandlerTypeName { get; } = handlerTypeName;

    /// <summary>Gets the declared name of the event the job carries.</summary>
    public string EventName { get; } = eventName;
}
