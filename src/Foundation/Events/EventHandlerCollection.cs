using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IOKode.OpinionatedFramework.Events;

/// <summary>
/// Associates one handler type with the event type it handles, together with the policy its driver applies.
/// </summary>
/// <remarks>
/// <see cref="Invoke"/> is built where the handler is declared, which is the only place where both types are
/// known statically. A driver iterating over registrations therefore calls the handler through a typed
/// delegate instead of reconstructing the call from <see cref="Type"/> values it was handed.
/// </remarks>
/// <typeparam name="TPolicy">The execution policy owned by the driver.</typeparam>
/// <param name="EventType">The event type the handler is registered for, possibly an event interface.</param>
/// <param name="HandlerType">The handler type implementing <see cref="IEventHandler{TEvent}"/>.</param>
/// <param name="Policy">How the driver executes the handler.</param>
/// <param name="Invoke">Resolves the handler from the supplied provider and calls it with the event.</param>
public sealed record EventHandlerRegistration<TPolicy>(
    Type EventType,
    Type HandlerType,
    TPolicy Policy,
    Func<IServiceProvider, IEvent, CancellationToken, Task> Invoke);

/// <summary>
/// Collects the handlers a driver executes.
/// </summary>
/// <remarks>
/// Handlers are declared on the options of the driver that runs them, so a process registers only the handlers
/// it is responsible for. This is what allows a subscriber to run in a different process from the dispatcher:
/// the dispatching side no longer needs to know the complete handler set.
/// </remarks>
/// <remarks>
/// The execution policy is owned by the driver rather than by this contract, because a driver applies it with
/// its own machinery and can only offer what that machinery supports. A knob a driver cannot express is absent
/// from its policy type, so declaring it does not compile instead of being quietly dropped.
/// </remarks>
/// <typeparam name="TPolicy">The execution policy owned by the driver.</typeparam>
public abstract class EventHandlerCollection<TPolicy> where TPolicy : new()
{
    private readonly List<EventHandlerRegistration<TPolicy>> registrations = [];

    /// <summary>Gets the registered handlers, in the order they were added.</summary>
    public IReadOnlyList<EventHandlerRegistration<TPolicy>> EventHandlerRegistrations => this.registrations;

    /// <summary>
    /// Registers <typeparamref name="THandler"/> to handle <typeparamref name="TEvent"/>.
    /// </summary>
    /// <remarks>
    /// A handler registered for an event interface receives every event assignable to it, which is how a
    /// handler that observes or stores every event is declared.
    /// </remarks>
    /// <typeparam name="TEvent">The event type to handle, possibly an interface grouping several.</typeparam>
    /// <typeparam name="THandler">The handler type.</typeparam>
    /// <param name="configure">Configures the execution policy, or <see langword="null"/> for the driver default.</param>
    /// <exception cref="MissingEventNameException">
    /// <typeparamref name="TEvent"/> is concrete and declares no <see cref="EventNameAttribute"/>.
    /// </exception>
    public void AddEventHandler<TEvent, THandler>(Action<TPolicy>? configure = null)
        where TEvent : ISubscribableEvent
        where THandler : class, IEventHandler<TEvent>
    {
        // An interface is a filter rather than something that travels, so only concrete types must be named.
        if (!typeof(TEvent).IsAbstract)
        {
            _ = EventName.Of<TEvent>();
        }

        var policy = new TPolicy();
        configure?.Invoke(policy);

        this.registrations.Add(new EventHandlerRegistration<TPolicy>(
            typeof(TEvent),
            typeof(THandler),
            policy,
            InvokeHandler));

        return;

        // Closes over the declared types, so the cast is the one the handler expects even when it was
        // registered against a base of the event that gets dispatched.
        static Task InvokeHandler(IServiceProvider serviceProvider, IEvent @event, CancellationToken cancellationToken)
        {
            // A provider without the handler is a state in which the call cannot mean anything, and this is
            // the exception the provider's own GetRequiredService reports it with. Calling that extension
            // method instead would make the contracts package depend on a dependency injection package.
            var handler = (THandler?) serviceProvider.GetService(typeof(THandler))
                ?? throw new InvalidOperationException(
                    $"The event handler '{typeof(THandler).FullName}' is not registered in the service container.");

            return handler.HandleAsync((TEvent) @event, cancellationToken);
        }
    }

    /// <summary>
    /// Gets the handlers registered for an event type, including those registered for an interface it implements.
    /// </summary>
    /// <param name="eventType">The concrete type of the dispatched event.</param>
    /// <exception cref="ArgumentNullException"><paramref name="eventType"/> is <see langword="null"/>.</exception>
    public IReadOnlyList<EventHandlerRegistration<TPolicy>> GetRegistrationsFor(Type eventType)
    {
        ArgumentNullException.ThrowIfNull(eventType);

        return this.registrations
            .Where(registration => registration.EventType.IsAssignableFrom(eventType))
            .ToArray();
    }
}
