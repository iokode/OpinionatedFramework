using System;
using System.Collections.Generic;
using System.Linq;
using IOKode.OpinionatedFramework.Drivers.Abstractions;
using IOKode.OpinionatedFramework.Events;

namespace IOKode.OpinionatedFramework.Internals.Events;

/// <summary>
/// Checks that no two declared event types share a name.
/// </summary>
/// <remarks>
/// The declared name is the identity of an event: it is what a payload carries, what a queue is named after,
/// and what a stored event is read back by. Two types answering to the same name make that identity ambiguous,
/// so every driver rejects it, and the rule lives here so the three of them cannot disagree about what counts
/// as a clash or about what the failure says.
/// </remarks>
/// <remarks>
/// Only the types a driver knows about are examined. Two events sharing a name where neither is registered nor
/// declared cannot be detected, because nothing has revealed them to the driver.
/// </remarks>
public static class EventNameUniqueness
{
    /// <summary>
    /// Reports each name declared by more than one event type as a configuration error.
    /// </summary>
    /// <param name="eventTypes">The concrete event types the driver knows about.</param>
    /// <param name="configurationPath">The configuration path the errors point at.</param>
    /// <exception cref="ArgumentNullException"><paramref name="eventTypes"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<BootstrapValidationError> Validate(
        IEnumerable<Type> eventTypes, string configurationPath)
    {
        return FindClashes(eventTypes)
            .Select(clash => new BootstrapValidationError(configurationPath, Describe(clash)))
            .ToArray();
    }

    /// <summary>
    /// Throws when a name is declared by more than one event type.
    /// </summary>
    /// <remarks>
    /// For the registration path, which an application can reach without going through bootstrap validation.
    /// </remarks>
    /// <param name="eventTypes">The concrete event types the driver knows about.</param>
    /// <exception cref="ArgumentNullException"><paramref name="eventTypes"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">A name is declared by more than one event type.</exception>
    public static void EnsureUnique(IEnumerable<Type> eventTypes)
    {
        var clashes = FindClashes(eventTypes);
        if (clashes.Count > 0)
        {
            throw new InvalidOperationException(string.Join(" ", clashes.Select(Describe)));
        }
    }

    private static IReadOnlyList<IGrouping<string, Type>> FindClashes(IEnumerable<Type> eventTypes)
    {
        ArgumentNullException.ThrowIfNull(eventTypes);

        return eventTypes
            .GroupBy(EventName.Of, StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .ToArray();
    }

    private static string Describe(IGrouping<string, Type> clash)
    {
        var declaringTypes = string.Join("', '", clash.Select(eventType => eventType.FullName));

        return $"The event name '{clash.Key}' is declared by more than one event type: '{declaringTypes}'. " +
               "An event name identifies the event beyond the running process, so it has to be unique.";
    }
}
