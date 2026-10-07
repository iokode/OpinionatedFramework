using System;

namespace IOKode.OpinionatedFramework.ContractImplementations.Hangfire.Exceptions;

/// <summary>
/// Thrown when a payload names an event type the driver was not told about.
/// </summary>
/// <remarks>
/// The name is looked up in a map holding every declared event, the ones this process raises as much as the
/// ones it reacts to, but a payload only ever carries one of the second kind: it was stored for a handler.
/// That is why the declaration named in the message is the handler's.
/// </remarks>
/// <param name="eventName">The name read from the payload.</param>
public sealed class UnknownEventNameException(string eventName)
    : Exception($"No declared event type carries the name '{eventName}'. The job was stored while the event " +
                "was declared, so either the declaration was removed or the name the event declares changed. " +
                "Declaring it again with the handler that reads it, 'events.Handles<TEvent, THandler>()', is " +
                "what makes the waiting jobs readable.")
{
    /// <summary>Gets the name read from the payload.</summary>
    public string EventName { get; } = eventName;
}
