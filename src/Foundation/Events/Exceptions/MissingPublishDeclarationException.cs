using System;

namespace IOKode.OpinionatedFramework.Events.Exceptions;

/// <summary>
/// Thrown when an event is dispatched that the application never declared it raises.
/// </summary>
/// <remarks>
/// Implementing <see cref="IPublishableEvent"/> says the event may be raised; the declaration says this
/// application raises it. Without the declaration the startup checks never examined the event and a driver
/// carrying it beyond the process was never told its name, so the dispatch is refused instead of being
/// performed on an event nothing validated.
/// </remarks>
/// <param name="eventType">The event type that was dispatched.</param>
public sealed class MissingPublishDeclarationException(Type eventType)
    : Exception($"The event type '{eventType.FullName}' was dispatched without this application declaring " +
                $"that it raises it. Add 'events.Publishes<{eventType.Name}>()' where the event declarations " +
                "of the selected driver are made.")
{
    /// <summary>Gets the event type that was dispatched.</summary>
    public Type EventType { get; } = eventType;
}
