using System;
using System.Text.Json;
using IOKode.OpinionatedFramework.Events;
using IOKode.OpinionatedFramework.Internals.Events;

namespace IOKode.OpinionatedFramework.ContractImplementations.Hangfire;

/// <summary>
/// Carries an event across a process boundary as its declared name plus a JSON body.
/// </summary>
/// <remarks>
/// The name rather than the CLR type is what travels, so a payload stays readable after the publishing
/// assembly is renamed or when the reader is a different application altogether.
/// </remarks>
/// <param name="Name">The stable name declared by the event type.</param>
/// <param name="Body">The event serialized as JSON.</param>
public sealed record EventPayload(string Name, string Body)
{
    /// <summary>
    /// Serializes an event using its concrete runtime type.
    /// </summary>
    /// <param name="event">The event to serialize.</param>
    /// <exception cref="ArgumentNullException"><paramref name="event"/> is <see langword="null"/>.</exception>
    /// <exception cref="MissingEventNameException">The concrete type declares no name.</exception>
    public static EventPayload From(IPublishableEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        // The concrete type, never the static one, so a variable typed as the base still round-trips.
        var eventType = @event.GetType();
        return new EventPayload(EventName.Of(eventType), EventSerializer.Serialize(@event, eventType));
    }

    /// <summary>
    /// Reconstructs the event this payload carries.
    /// </summary>
    /// <param name="typeMap">The map resolving the declared name to a type.</param>
    /// <exception cref="ArgumentNullException"><paramref name="typeMap"/> is <see langword="null"/>.</exception>
    /// <exception cref="UnknownEventNameException">The name is not known to the driver.</exception>
    /// <exception cref="MalformedEventPayloadException">The body does not read back into the named event type.</exception>
    public IEvent ToEvent(EventTypeMap typeMap)
    {
        ArgumentNullException.ThrowIfNull(typeMap);

        var eventType = typeMap.Resolve(this.Name);

        // The map only holds event types, so the cast cannot fail; what can is the body, either by being
        // unreadable or by being the JSON null, and both are the same failure to whoever reads the payload.
        try
        {
            return EventSerializer.Deserialize(this.Body, eventType)
                   ?? throw new MalformedEventPayloadException(this.Name, eventType);
        }
        catch (JsonException exception)
        {
            throw new MalformedEventPayloadException(this.Name, eventType, exception);
        }
    }
}

/// <summary>
/// Thrown when a payload does not read back into the event type its name resolves to.
/// </summary>
/// <remarks>
/// The payload is data this driver stored earlier, so a body it cannot read means the stored event is lost to
/// the handler that was waiting for it, and the job fails with the name and the type it was trying to rebuild.
/// </remarks>
/// <param name="eventName">The name the payload carries.</param>
/// <param name="eventType">The event type the name resolves to.</param>
/// <param name="innerException">What the serializer reported, when it reported anything.</param>
public sealed class MalformedEventPayloadException(
    string eventName, Type eventType, Exception? innerException = null)
    : Exception($"The payload for event '{eventName}' could not be read back into '{eventType.FullName}'.",
        innerException)
{
    /// <summary>Gets the name the payload carries.</summary>
    public string EventName { get; } = eventName;

    /// <summary>Gets the event type the name resolves to.</summary>
    public Type EventType { get; } = eventType;
}
