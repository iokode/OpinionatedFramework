using System;

namespace IOKode.OpinionatedFramework.ContractImplementations.Hangfire.Exceptions;

/// <summary>
/// Thrown when a payload names an event type the driver was not told about.
/// </summary>
/// <param name="eventName">The name read from the payload.</param>
public sealed class UnknownEventNameException(string eventName)
    : Exception($"No declared event type carries the name '{eventName}'. " +
                "Declare the event with 'events.Handles<TEvent, THandler>()'.")
{
    /// <summary>Gets the name read from the payload.</summary>
    public string EventName { get; } = eventName;
}
