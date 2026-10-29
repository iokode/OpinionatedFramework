using System;
using System.Text.Json;
using IOKode.OpinionatedFramework.Events;

namespace IOKode.OpinionatedFramework.Internals.Events;

/// <summary>
/// Decides whether a driver raises <see cref="EventHandlerExhausted"/> and builds it.
/// </summary>
/// <remarks>
/// Where a driver notices that a handler has no attempts left is its own, because each one retries with its own
/// machinery. What happens then is shared, so the event is raised in the same cases whichever driver is selected.
/// </remarks>
public static class EventHandlerExhaustion
{
    /// <summary>
    /// Gets whether the exhaustion of a handler of the named event is raised.
    /// </summary>
    /// <remarks>
    /// It is not when the application did not opt into it, and not for a handler of
    /// <see cref="EventHandlerExhausted"/> itself, whose failure would otherwise raise it again.
    /// </remarks>
    /// <param name="declarations">The event declarations of the driver.</param>
    /// <param name="failedEventName">The name of the event the handler failed on.</param>
    /// <exception cref="ArgumentNullException">Any argument is <see langword="null"/>.</exception>
    public static bool IsRaisedFor<TPolicy>(EventDeclarations<TPolicy> declarations, string failedEventName)
        where TPolicy : new()
    {
        ArgumentNullException.ThrowIfNull(declarations);
        ArgumentNullException.ThrowIfNull(failedEventName);

        return declarations.IsDeclaredAsPublished(typeof(EventHandlerExhausted))
               && failedEventName != EventName.Of<EventHandlerExhausted>();
    }

    /// <summary>
    /// Builds the event that reports the exhaustion.
    /// </summary>
    /// <param name="failedEventName">The name of the event the handler failed on.</param>
    /// <param name="failedEventBody">That event as JSON.</param>
    /// <param name="handlerName">The full name of the handler type.</param>
    /// <param name="attempts">How many attempts were made, the first one included.</param>
    /// <exception cref="ArgumentNullException">A string argument is <see langword="null"/>.</exception>
    /// <exception cref="JsonException"><paramref name="failedEventBody"/> is not JSON.</exception>
    public static EventHandlerExhausted Describe(string failedEventName, string failedEventBody, string handlerName,
        int attempts)
    {
        ArgumentNullException.ThrowIfNull(failedEventName);
        ArgumentNullException.ThrowIfNull(failedEventBody);
        ArgumentNullException.ThrowIfNull(handlerName);

        return new EventHandlerExhausted
        {
            EventName = failedEventName,
            EventBody = JsonDocument.Parse(failedEventBody),
            HandlerName = handlerName,
            Attempts = attempts
        };
    }
}
