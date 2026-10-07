using System;

namespace IOKode.OpinionatedFramework.Events.Exceptions;

/// <summary>
/// Thrown when a concrete event type does not declare the required <see cref="EventNameAttribute"/>.
/// </summary>
/// <param name="eventType">The event type missing the attribute.</param>
public sealed class MissingEventNameException(Type eventType)
    : Exception($"The event type '{eventType.FullName}' must declare an [EventName] attribute.")
{
    /// <summary>Gets the event type missing the attribute.</summary>
    public Type EventType { get; } = eventType;
}
