using IOKode.OpinionatedFramework.Jobs;

namespace IOKode.OpinionatedFramework.ContractImplementations.Hangfire;

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
