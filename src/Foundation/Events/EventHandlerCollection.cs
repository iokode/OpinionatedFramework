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
/// <param name="EventType">The concrete event type the handler is declared for.</param>
/// <param name="HandlerType">The handler type implementing <see cref="IEventHandler{TEvent}"/>.</param>
/// <param name="Policy">How the driver executes the handler.</param>
/// <param name="Invoke">Resolves the handler from the supplied provider and calls it with the event.</param>
public sealed record EventHandlerRegistration<TPolicy>(
    Type EventType,
    Type HandlerType,
    TPolicy Policy,
    Func<IServiceProvider, IEvent, CancellationToken, Task> Invoke);

/// <summary>
/// Collects what this application does with events: the ones it reacts to, with what, and the ones it raises.
/// </summary>
/// <remarks>
/// The declarations live on the options of the driver that carries them, so a process declares only what it is
/// itself responsible for. This is what allows a subscriber to run in a different process from the dispatcher:
/// the dispatching side no longer needs to know the complete handler set.
/// </remarks>
/// <remarks>
/// The interfaces an event implements say what may be done with it; a declaration here says what this
/// application does with it. The two are not the same thing, so an event that is both publishable and
/// subscribable is declared twice, once with <see cref="Handles{TEvent,THandler}"/> and once with
/// <see cref="Publishes{TEvent}"/>: reacting to an event and raising it are separate responsibilities, and a
/// process often takes only one of them.
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
    private readonly List<Type> publishedEventTypes = [];

    /// <summary>Gets the declared handlers, in the order they were declared.</summary>
    public IReadOnlyList<EventHandlerRegistration<TPolicy>> EventHandlerRegistrations => this.registrations;

    /// <summary>
    /// Gets every event type this application declared, whether it reacts to it, raises it, or both.
    /// </summary>
    /// <remarks>
    /// This is the whole set a driver was told about, which is what it validates at startup and, when it
    /// carries an event beyond the process, what it has to be able to name and rebuild.
    /// </remarks>
    public IReadOnlyList<Type> DeclaredEventTypes =>
        this.registrations
            .Select(registration => registration.EventType)
            .Concat(this.publishedEventTypes)
            .Distinct()
            .ToArray();

    /// <summary>
    /// Declares that this application reacts to <typeparamref name="TEvent"/> with <typeparamref name="THandler"/>.
    /// </summary>
    /// <remarks>
    /// <typeparamref name="TEvent"/> is the concrete event and never an interface grouping several of them: an
    /// interface declares no name, nothing can be rebuilt into it, and a queue cannot be bound to it. A handler
    /// written against a group of events is therefore declared once per event it covers, which is what
    /// <see cref="IEventHandler{TEvent}"/> being contravariant allows.
    /// </remarks>
    /// <typeparam name="TEvent">The concrete event type to react to.</typeparam>
    /// <typeparam name="THandler">The handler type.</typeparam>
    /// <param name="configure">Configures the execution policy, or <see langword="null"/> for the driver default.</param>
    /// <exception cref="MissingEventNameException">
    /// <typeparamref name="TEvent"/> declares no <see cref="EventNameAttribute"/>, which is what naming an
    /// event interface here fails with too.
    /// </exception>
    public void Handles<TEvent, THandler>(Action<TPolicy>? configure = null)
        where TEvent : ISubscribableEvent
        where THandler : class, IEventHandler<TEvent>
    {
        _ = EventName.Of<TEvent>();

        var policy = new TPolicy();
        configure?.Invoke(policy);

        this.registrations.Add(new EventHandlerRegistration<TPolicy>(
            typeof(TEvent),
            typeof(THandler),
            policy,
            InvokeHandler));

        return;

        // Closes over the declared types, so the handler is called through the event type it was declared for
        // even when the handler itself is written against a group of events.
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
    /// Declares that this application raises <typeparamref name="TEvent"/>.
    /// </summary>
    /// <remarks>
    /// Dispatching an event that was not declared here fails, so what a process raises is stated rather than
    /// inferred from which types happen to be reachable from it.
    /// </remarks>
    /// <remarks>
    /// It is also what reveals an event no handler covers. Without it, the startup checks of a driver would
    /// never see an event the application only raises, and a driver that carries one beyond the process would
    /// not know the name to put on the wire.
    /// </remarks>
    /// <typeparam name="TEvent">The concrete event type this application raises.</typeparam>
    /// <exception cref="MissingEventNameException">
    /// <typeparamref name="TEvent"/> declares no <see cref="EventNameAttribute"/>.
    /// </exception>
    public void Publishes<TEvent>() where TEvent : IPublishableEvent
    {
        _ = EventName.Of<TEvent>();

        if (!this.publishedEventTypes.Contains(typeof(TEvent)))
        {
            this.publishedEventTypes.Add(typeof(TEvent));
        }
    }

    /// <summary>
    /// Gets the handlers declared for an event type.
    /// </summary>
    /// <param name="eventType">The concrete type of the dispatched event.</param>
    /// <exception cref="ArgumentNullException"><paramref name="eventType"/> is <see langword="null"/>.</exception>
    public IReadOnlyList<EventHandlerRegistration<TPolicy>> GetRegistrationsFor(Type eventType)
    {
        ArgumentNullException.ThrowIfNull(eventType);

        return this.registrations
            .Where(registration => registration.EventType == eventType)
            .ToArray();
    }

    /// <summary>
    /// Throws unless the dispatched event was declared with <see cref="Publishes{TEvent}"/>.
    /// </summary>
    /// <remarks>
    /// Every dispatcher checks this before it carries an event anywhere, so an event raised by an application
    /// that never said it raises it fails the same way whichever driver is selected.
    /// </remarks>
    /// <param name="eventType">The concrete type of the dispatched event.</param>
    /// <exception cref="ArgumentNullException"><paramref name="eventType"/> is <see langword="null"/>.</exception>
    /// <exception cref="MissingPublishDeclarationException">The event type was not declared.</exception>
    public void EnsureDeclaredAsPublished(Type eventType)
    {
        ArgumentNullException.ThrowIfNull(eventType);

        if (!this.publishedEventTypes.Contains(eventType))
        {
            throw new MissingPublishDeclarationException(eventType);
        }
    }
}

/// <summary>
/// Thrown when an event is dispatched that the application never declared it raises.
/// </summary>
/// <remarks>
/// Implementing <see cref="IPublishableEvent"/> says the event may be raised; the declaration says this
/// application raises it. Without the declaration the startup checks never examined the event and a driver
/// carrying it beyond the process was never told its name, so the dispatch is refused instead of being
/// performed on an event nothing validated.
/// </remarks>
/// <param name="eventType">The event type that was dispatched.</param>
public sealed class MissingPublishDeclarationException(Type eventType)
    : Exception($"The event type '{eventType.FullName}' was dispatched without this application declaring " +
                $"that it raises it. Add 'events.Publishes<{eventType.Name}>()' where the event declarations " +
                "of the selected driver are made.")
{
    /// <summary>Gets the event type that was dispatched.</summary>
    public Type EventType { get; } = eventType;
}
