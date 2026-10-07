using System;

namespace IOKode.OpinionatedFramework.ContractImplementations.Hangfire.Exceptions;

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
