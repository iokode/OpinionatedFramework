using System;
using System.Collections.Generic;
using IOKode.OpinionatedFramework.Events;
using IOKode.OpinionatedFramework.Internals.Events;

namespace IOKode.OpinionatedFramework.ContractImplementations.Hangfire;

/// <summary>
/// Declares the handlers the Hangfire event dispatcher enqueues and runs.
/// </summary>
/// <remarks>
/// This driver rebuilds the event from the job payload, so it also needs to know the concrete event types it
/// may receive. That requirement is the driver's, not the contract's, which is why the declaration lives here.
/// </remarks>
public sealed class HangfireEventsOptions : EventHandlerCollection<HangfireEventHandlerPolicy>
{
    private readonly ConcreteEventTypes concreteEvents = new();

    /// <summary>
    /// Declares a concrete event type this driver may receive.
    /// </summary>
    /// <remarks>
    /// Only needed for an event reachable solely through a handler registered against an event interface: an
    /// interface does not reveal which concrete events implement it, and the job payload has to be rebuilt into
    /// a type that can be constructed.
    /// </remarks>
    /// <typeparam name="TEvent">The concrete event type.</typeparam>
    /// <exception cref="MissingEventNameException"><typeparamref name="TEvent"/> declares no <see cref="EventNameAttribute"/>.</exception>
    public void AddEvent<TEvent>() where TEvent : ISubscribableEvent => this.concreteEvents.Declare<TEvent>();

    /// <summary>
    /// Gets every concrete event type this driver may receive: those declared with
    /// <see cref="AddEvent{TEvent}"/> and those a handler was registered for directly.
    /// </summary>
    /// <remarks>
    /// Public because the driver reads it while validating and registering, not because an application has a
    /// reason to call it.
    /// </remarks>
    public IReadOnlyList<Type> ResolveConcreteEventTypes() => this.concreteEvents.Resolve(this);
}

/// <summary>
/// Holds the event type map built from the declared events, so a running job can read an event back.
/// </summary>
/// <param name="Value">The map from declared event name to type.</param>
public sealed record HangfireEventTypeMap(EventTypeMap Value);
