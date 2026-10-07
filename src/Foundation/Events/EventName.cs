using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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

/// <summary>
/// Thrown when more than one event type declares the same name.
/// </summary>
/// <remarks>
/// A name identifies an event beyond the running process, so two types answering to the same one make a stored
/// or transported event ambiguous. Which types clash is part of the failure, so they are carried here rather
/// than only described in the message.
/// </remarks>
/// <param name="eventName">The name declared more than once.</param>
/// <param name="declaringTypes">The event types declaring it.</param>
public sealed class DuplicateEventNameException(string eventName, IEnumerable<Type> declaringTypes)
    : Exception($"The event name '{eventName}' is declared by more than one event type: " +
                $"'{string.Join("', '", declaringTypes.Select(type => type.FullName))}'. " +
                "An event name identifies the event beyond the running process, so it has to be unique.")
{
    /// <summary>Gets the name declared more than once.</summary>
    public string EventName { get; } = eventName;

    /// <summary>Gets the event types declaring it.</summary>
    public IReadOnlyList<Type> DeclaringTypes { get; } = declaringTypes.ToArray();
}
