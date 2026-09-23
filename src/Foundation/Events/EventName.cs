using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace IOKode.OpinionatedFramework.Events;

/// <summary>
/// Resolves the stable name declared by an event type.
/// </summary>
public static class EventName
{
    private static readonly ConcurrentDictionary<Type, string> names = new();

    /// <summary>
    /// Gets the name declared by <typeparamref name="TEvent"/>.
    /// </summary>
    /// <typeparam name="TEvent">The event type.</typeparam>
    /// <exception cref="MissingEventNameException">The type declares no <see cref="EventNameAttribute"/>.</exception>
    public static string Of<TEvent>() where TEvent : IEvent
    {
        return Of(typeof(TEvent));
    }

    /// <summary>
    /// Gets the name declared by <paramref name="eventType"/>.
    /// </summary>
    /// <param name="eventType">The event type.</param>
    /// <exception cref="ArgumentNullException"><paramref name="eventType"/> is <see langword="null"/>.</exception>
    /// <exception cref="MissingEventNameException">The type declares no <see cref="EventNameAttribute"/>.</exception>
    public static string Of(Type eventType)
    {
        ArgumentNullException.ThrowIfNull(eventType);

        return names.GetOrAdd(eventType, static type =>
            Declared(type) ?? throw new MissingEventNameException(type));
    }

    /// <remarks>
    /// The attribute is not inherited, so an interface used only to group or filter events does not take the
    /// name of anything.
    /// </remarks>
    private static string? Declared(Type eventType)
    {
        return eventType.GetCustomAttribute<EventNameAttribute>(inherit: false)?.Name;
    }
}

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
