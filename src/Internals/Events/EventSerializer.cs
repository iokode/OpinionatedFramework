using System;
using System.Text.Json;
using IOKode.OpinionatedFramework.Events;

namespace IOKode.OpinionatedFramework.Internals.Events;

/// <summary>
/// Writes an event as JSON and reads it back.
/// </summary>
/// <remarks>
/// Offered to an implementation that has to produce the representation of an event itself, such as one storing
/// it as part of a job. It is not imposed on anyone: an implementation whose technology carries the event as
/// its own message serializes it with that technology, because the message model of a transport is its own and
/// replacing it would cost what the transport is used for.
/// </remarks>
/// <remarks>
/// The event name is not written here. What identifies an event is the name it declares, and whoever carries
/// an event stores that name next to this representation, so nothing about the CLR type is needed to read it
/// back.
/// </remarks>
public static class EventSerializer
{
    /// <remarks>
    /// Camel case and case-insensitive reading, which is what the JSON conventions of every technology
    /// involved already use, so an event written here stays readable by a reader that is not this one.
    /// </remarks>
    private static readonly JsonSerializerOptions options = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Writes an event as JSON.
    /// </summary>
    /// <param name="event">The event to write.</param>
    /// <param name="eventType">
    /// Its concrete type, which is what decides the members written. A type taken from the variable holding the
    /// event would write only what that type declares.
    /// </param>
    /// <exception cref="ArgumentNullException">Any argument is <see langword="null"/>.</exception>
    public static string Serialize(object @event, Type eventType)
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentNullException.ThrowIfNull(eventType);

        return JsonSerializer.Serialize(@event, eventType, options);
    }

    /// <summary>
    /// Reads an event back from what <see cref="Serialize"/> wrote.
    /// </summary>
    /// <param name="body">The JSON written earlier.</param>
    /// <param name="eventType">The event type to read it into.</param>
    /// <returns>The event, or <see langword="null"/> when the body is the JSON null.</returns>
    /// <exception cref="ArgumentNullException">Any argument is <see langword="null"/>.</exception>
    /// <exception cref="JsonException">The body does not read into that type.</exception>
    public static IEvent? Deserialize(string body, Type eventType)
    {
        ArgumentNullException.ThrowIfNull(body);
        ArgumentNullException.ThrowIfNull(eventType);

        return (IEvent?) JsonSerializer.Deserialize(body, eventType, options);
    }

    /// <summary>
    /// Writes an event and reads it back, for an implementation checking that it can carry one.
    /// </summary>
    /// <remarks>
    /// Shaped to be handed to <see cref="EventSerializability.Validate"/> by an implementation that carries
    /// events with this serializer.
    /// </remarks>
    /// <param name="event">The event to try.</param>
    /// <param name="eventType">Its concrete type.</param>
    public static void RoundTrip(object @event, Type eventType)
    {
        var serialized = Serialize(@event, eventType);
        Deserialize(serialized, eventType);
    }
}
