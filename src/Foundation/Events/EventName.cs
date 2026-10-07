using System;
using System.Collections.Concurrent;
using System.Reflection;
using IOKode.OpinionatedFramework.Events.Exceptions;

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

    /// <summary>
    /// Gets the name declared by <paramref name="eventType"/>, or <see langword="null"/> when it declares none.
    /// </summary>
    /// <remarks>
    /// For a caller that is handed types beyond the application's events and has to tell which ones are
    /// events, such as a driver naming the application's events alongside the messages its broker invents.
    /// </remarks>
    /// <remarks>
    /// The attribute is not inherited, so an interface an event implements does not take the name of anything.
    /// </remarks>
    /// <param name="eventType">The type to read the name from.</param>
    /// <exception cref="ArgumentNullException"><paramref name="eventType"/> is <see langword="null"/>.</exception>
    public static string? Declared(Type eventType)
    {
        ArgumentNullException.ThrowIfNull(eventType);

        return eventType.GetCustomAttribute<EventNameAttribute>(inherit: false)?.Name;
    }
}
