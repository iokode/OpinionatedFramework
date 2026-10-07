using System;
using System.Collections.Generic;
using System.Linq;

namespace IOKode.OpinionatedFramework.Events.Exceptions;

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
