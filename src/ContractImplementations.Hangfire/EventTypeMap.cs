using System;
using System.Collections.Generic;
using IOKode.OpinionatedFramework.Events;

namespace IOKode.OpinionatedFramework.ContractImplementations.Hangfire;

/// <summary>
/// Resolves an event type from the stable name a payload carries.
/// </summary>
/// <remarks>
/// A dispatcher that crosses a process boundary writes the declared name into the payload, never the CLR type,
/// so reading the event back requires a map built from the types the driver was told about.
/// </remarks>
public sealed class EventTypeMap
{
    private readonly Dictionary<string, Type> typesByName = new(StringComparer.Ordinal);

    /// <summary>
    /// Builds the map from the event types a driver may receive.
    /// </summary>
    /// <remarks>
    /// The names are expected to be unique, which the driver has already established with
    /// <c>EventNameUniqueness</c> before reaching this point.
    /// </remarks>
    /// <param name="eventTypes">The concrete event types.</param>
    /// <exception cref="ArgumentNullException"><paramref name="eventTypes"/> is <see langword="null"/>.</exception>
    public EventTypeMap(IEnumerable<Type> eventTypes)
    {
        ArgumentNullException.ThrowIfNull(eventTypes);

        foreach (var eventType in eventTypes)
        {
            this.typesByName[EventName.Of(eventType)] = eventType;
        }
    }

    /// <summary>
    /// Gets the event type declaring <paramref name="eventName"/>.
    /// </summary>
    /// <param name="eventName">The stable name read from a payload.</param>
    /// <exception cref="UnknownEventNameException">No known event type declares that name.</exception>
    public Type Resolve(string eventName)
    {
        if (!this.typesByName.TryGetValue(eventName, out var eventType))
        {
            throw new UnknownEventNameException(eventName);
        }

        return eventType;
    }
}

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
