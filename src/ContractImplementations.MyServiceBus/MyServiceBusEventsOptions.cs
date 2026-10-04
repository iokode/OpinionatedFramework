using System;
using System.Collections.Generic;
using IOKode.OpinionatedFramework.Events;
using IOKode.OpinionatedFramework.Internals.Events;

namespace IOKode.OpinionatedFramework.ContractImplementations.MyServiceBus;

/// <summary>
/// Declares the handlers this process subscribes to the broker.
/// </summary>
/// <remarks>
/// Unlike the in-memory driver, only the handlers a process is responsible for are declared here. A publishing
/// process declares none, and a subscriber declares its own without the publisher knowing: the fan-out happens
/// at the broker rather than at the dispatcher.
/// </remarks>
/// <remarks>
/// The transport constructs the event it receives, so this driver also needs to know the concrete event types it
/// may be delivered. That requirement is the driver's, not the contract's, which is why the declaration lives
/// here.
/// </remarks>
public sealed class MyServiceBusEventsOptions : EventHandlerCollection<MyServiceBusEventHandlerPolicy>
{
    private readonly ConcreteEventTypes concreteEvents = new();

    /// <summary>
    /// Declares a concrete event type this process may be delivered.
    /// </summary>
    /// <remarks>
    /// Only needed for an event reachable solely through a handler registered against an event interface: a
    /// queue cannot be bound to an interface the transport would be unable to construct, so the interface
    /// registration is subscribed once per concrete event instead.
    /// </remarks>
    /// <typeparam name="TEvent">The concrete event type.</typeparam>
    /// <exception cref="MissingEventNameException"><typeparamref name="TEvent"/> declares no <see cref="EventNameAttribute"/>.</exception>
    public void AddEvent<TEvent>() where TEvent : ISubscribableEvent => this.concreteEvents.Declare<TEvent>();

    /// <summary>
    /// Gets every concrete event type this driver may receive: those declared with
    /// <see cref="AddEvent{TEvent}"/> and those a handler was registered for directly.
    /// </summary>
    /// <remarks>
    /// Public because the transport package reads it while validating and registering, not because an application has a
    /// reason to call it.
    /// </remarks>
    public IReadOnlyList<Type> ResolveConcreteEventTypes() => this.concreteEvents.Resolve(this);
}
