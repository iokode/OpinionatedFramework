using System;

namespace IOKode.OpinionatedFramework.ContractImplementations.Hangfire.Exceptions;

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
