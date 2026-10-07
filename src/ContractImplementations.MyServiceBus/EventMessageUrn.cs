using System;
using IOKode.OpinionatedFramework.Events;

namespace IOKode.OpinionatedFramework.ContractImplementations.MyServiceBus;

/// <summary>
/// Builds the wire identity MyServiceBus writes into the envelope of an event.
/// </summary>
/// <remarks>
/// The identity is what a consumer matches a delivery against, so it is derived from the declared event name
/// and never from the CLR type: two applications that each declare their own type for the same event agree on
/// it, and renaming a namespace does not make an in-flight event unrecognizable.
/// </remarks>
/// <remarks>
/// The <c>urn:message:</c> prefix is the one MyServiceBus and MassTransit put in front of every identity they
/// derive themselves, and the library strips it again when it composes the identity of a fault around an
/// event's. Keeping it leaves both conventions intact.
/// </remarks>
public static class EventMessageUrn
{
    /// <summary>
    /// Builds the wire identity of a concrete event type.
    /// </summary>
    /// <param name="eventType">The concrete event type.</param>
    /// <exception cref="ArgumentNullException"><paramref name="eventType"/> is <see langword="null"/>.</exception>
    /// <exception cref="MissingEventNameException">The event type declares no name.</exception>
    public static string For(Type eventType)
    {
        ArgumentNullException.ThrowIfNull(eventType);

        return $"urn:message:{EventName.Of(eventType)}";
    }
}
