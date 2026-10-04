using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using IOKode.OpinionatedFramework.Drivers.Abstractions;
using IOKode.OpinionatedFramework.Events;

namespace IOKode.OpinionatedFramework.Internals.Events;

/// <summary>
/// Checks that a driver can carry the events it may be given and read them back.
/// </summary>
/// <remarks>
/// A driver that carries an event beyond the running process needs it to survive a round trip, and an event
/// that cannot is a mistake in the application rather than something the driver can recover from. Checking it
/// while validating turns it into a failure to start, instead of a dispatch that fails much later with the
/// event already on its way.
/// </remarks>
/// <remarks>
/// How an event is carried is the driver's own business, so the round trip is supplied by the driver and
/// nothing here knows which serializer, format or library performs it. What is shared is reaching an instance
/// to try it on, examining every declared event, and wording the failure. A driver that never carries an event
/// beyond the process has nothing to check.
/// </remarks>
/// <remarks>
/// The round trip is tried on an instance built without running the event's constructors, because an event
/// declares its members as required and no sensible value can be invented for them. Every member is therefore
/// at its default value, which is enough to catch an event the driver could not write or could not rebuild.
/// It says nothing about whether a populated event keeps its values, which only the application can know.
/// </remarks>
public static class EventSerializability
{
    /// <summary>
    /// Reports every event the driver cannot carry and read back as a configuration error.
    /// </summary>
    /// <param name="eventTypes">The concrete event types the driver knows about.</param>
    /// <param name="roundTrip">
    /// Writes the event the way the driver carries it and reads it back, throwing when either step fails. It
    /// receives the event and its concrete type, and what it returns is of no interest.
    /// </param>
    /// <param name="configurationPath">The configuration path the errors point at.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="eventTypes"/> or <paramref name="roundTrip"/> is <see langword="null"/>.
    /// </exception>
    public static IReadOnlyList<BootstrapValidationError> Validate(IEnumerable<Type> eventTypes,
        Action<object, Type> roundTrip, string configurationPath)
    {
        ArgumentNullException.ThrowIfNull(eventTypes);
        ArgumentNullException.ThrowIfNull(roundTrip);

        return eventTypes
            .Select(eventType => Describe(eventType, roundTrip))
            .OfType<string>()
            .Select(message => new BootstrapValidationError(configurationPath, message))
            .ToArray();
    }

    /// <summary>
    /// Tries the round trip on one event type.
    /// </summary>
    /// <returns>What is wrong with it, or <see langword="null"/> when it survives the round trip.</returns>
    private static string? Describe(Type eventType, Action<object, Type> roundTrip)
    {
        object instance;
        try
        {
            // Bypasses the constructors, which is the only way to reach an instance of a type whose members
            // are required. Every member is left at its default value.
            instance = RuntimeHelpers.GetUninitializedObject(eventType);
        }
        catch (Exception exception)
        {
            return $"{Name(eventType)} could not be instantiated to check whether this driver can carry it: " +
                   $"{exception.Message}";
        }

        try
        {
            roundTrip(instance, eventType);
            return null;
        }
        catch (Exception exception)
        {
            return $"{Name(eventType)} cannot be carried by this driver: {exception.Message} The check writes " +
                   "an event whose members are all at their default value and reads it back, so a member that " +
                   "cannot be read in that state fails here and has to be excluded from serialization.";
        }
    }

    private static string Name(Type eventType) =>
        $"The event '{EventName.Of(eventType)}' ('{eventType.FullName}')";
}
