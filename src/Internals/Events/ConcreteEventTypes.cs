using System;
using System.Collections.Generic;
using System.Linq;
using IOKode.OpinionatedFramework.Events;

namespace IOKode.OpinionatedFramework.Internals.Events;

/// <summary>
/// The concrete event types a driver must be able to reconstruct.
/// </summary>
/// <remarks>
/// A driver that carries an event across a process boundary rebuilds it from its declared name, so it needs to
/// know the types it may receive. This is not a concern of the contract: a driver that runs handlers in the
/// same process never reconstructs anything and needs none of this, which is why the declaration lives here and
/// is exposed only by the drivers that require it.
/// </remarks>
/// <remarks>
/// Registering a handler for a concrete type already reveals that type. Only an event reachable solely through
/// a handler registered against an interface has to be declared, because an interface does not reveal which
/// concrete events implement it.
/// </remarks>
public sealed class ConcreteEventTypes
{
    private readonly List<Type> declared = [];

    /// <summary>
    /// Declares a concrete event type the driver may receive.
    /// </summary>
    /// <typeparam name="TEvent">The concrete event type.</typeparam>
    /// <exception cref="MissingEventNameException"><typeparamref name="TEvent"/> declares no <see cref="EventNameAttribute"/>.</exception>
    public void Declare<TEvent>() where TEvent : ISubscribableEvent
    {
        _ = EventName.Of<TEvent>();

        if (!this.declared.Contains(typeof(TEvent)))
        {
            this.declared.Add(typeof(TEvent));
        }
    }

    /// <summary>
    /// Gets every concrete event type the driver may receive: those declared here and those a handler was
    /// registered for directly.
    /// </summary>
    /// <typeparam name="TPolicy">The execution policy owned by the calling driver.</typeparam>
    /// <param name="handlers">The declarations the application supplied to the driver.</param>
    /// <exception cref="ArgumentNullException"><paramref name="handlers"/> is <see langword="null"/>.</exception>
    public IReadOnlyList<Type> Resolve<TPolicy>(EventHandlerCollection<TPolicy> handlers)
        where TPolicy : new()
    {
        return this.declared.Concat(RegisteredIn(handlers)).Distinct().ToArray();
    }

    /// <summary>
    /// Gets the concrete event types a handler was registered for directly, ignoring interface registrations.
    /// </summary>
    /// <remarks>
    /// This is all a driver executing handlers in the current process ever needs, because it matches a
    /// dispatched event against the registrations by its runtime type.
    /// </remarks>
    /// <typeparam name="TPolicy">The execution policy owned by the calling driver.</typeparam>
    /// <param name="handlers">The declarations the application supplied to the driver.</param>
    /// <exception cref="ArgumentNullException"><paramref name="handlers"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<Type> RegisteredIn<TPolicy>(EventHandlerCollection<TPolicy> handlers)
        where TPolicy : new()
    {
        ArgumentNullException.ThrowIfNull(handlers);

        return handlers.EventHandlerRegistrations
            .Select(registration => registration.EventType)
            .Where(eventType => !eventType.IsAbstract)
            .Distinct()
            .ToArray();
    }
}
