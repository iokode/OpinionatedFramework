using System.Text.Json;

namespace IOKode.OpinionatedFramework.Events;

/// <summary>
/// Raised by the events driver when a handler fails and its execution policy has no attempts left.
/// </summary>
/// <remarks>
/// It is an event like any other: it goes through <see cref="IEventDispatcher"/> and reaches whatever the selected
/// driver reaches, another application subscribed through a broker included. Raising it is opt-in, so a driver
/// raises it only for an application that declared it with
/// <see cref="Extensions.BuiltInEventDeclarations.PublishesBuiltInEvents{TPolicy}"/>.
/// </remarks>
/// <remarks>
/// The failed event is carried as its name and its JSON rather than as an instance, so a subscriber reads this
/// event without declaring the one that failed. A subscriber that does declare it rebuilds it from the name.
/// </remarks>
/// <remarks>
/// It is raised once per exhaustion, never for a handler of this event itself, which is what keeps it from
/// raising itself in a loop, and never for an execution that was cancelled, because cancellation is not
/// exhaustion.
/// </remarks>
[EventName("opinionated-framework.event-handler-exhausted")]
public sealed class EventHandlerExhausted : IPublishableEvent, ISubscribableEvent
{
    /// <summary>Gets the name of the event the handler failed on.</summary>
    public required string EventName { get; init; }

    /// <summary>Gets the event the handler failed on, as JSON.</summary>
    public required JsonDocument EventBody { get; init; }

    /// <summary>Gets the full name of the handler type.</summary>
    public required string HandlerName { get; init; }

    /// <summary>Gets how many attempts were made, the first one included.</summary>
    /// <remarks>
    /// They are the attempts of the round that was exhausted. An execution started again after an exhaustion, as
    /// a failed job requeued by hand is, starts a new round, and its exhaustion counts only its own attempts.
    /// </remarks>
    public required int Attempts { get; init; }
}
