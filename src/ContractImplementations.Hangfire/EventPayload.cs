using System;
using System.Text.Json;
using IOKode.OpinionatedFramework.Events;

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
    private static readonly JsonSerializerOptions serializerOptions = new(JsonSerializerDefaults.Web);

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
        return new EventPayload(EventName.Of(eventType), JsonSerializer.Serialize(@event, eventType, serializerOptions));
    }

    /// <summary>
    /// Reconstructs the event this payload carries.
    /// </summary>
    /// <param name="typeMap">The map resolving the declared name to a type.</param>
    /// <exception cref="ArgumentNullException"><paramref name="typeMap"/> is <see langword="null"/>.</exception>
    /// <exception cref="UnknownEventNameException">The name is not known to the driver.</exception>
    /// <exception cref="InvalidOperationException">The body does not deserialize into the named event type.</exception>
    public IEvent ToEvent(EventTypeMap typeMap)
    {
        ArgumentNullException.ThrowIfNull(typeMap);

        var eventType = typeMap.Resolve(this.Name);
        var @event = JsonSerializer.Deserialize(this.Body, eventType, serializerOptions) as IEvent;

        return @event ?? throw new InvalidOperationException(
            $"The payload for event '{this.Name}' did not deserialize into '{eventType.FullName}'.");
    }
}
