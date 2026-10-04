using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IOKode.OpinionatedFramework.Drivers.Abstractions;
using IOKode.OpinionatedFramework.Internals.Events;
using MyServiceBus;
using MyServiceBus.Serialization;

namespace IOKode.OpinionatedFramework.ContractImplementations.MyServiceBus;

/// <summary>
/// Checks an event against the serialization MyServiceBus performs on it.
/// </summary>
/// <remarks>
/// This driver hands MyServiceBus the event itself and MyServiceBus writes it, so the check writes and reads
/// the event with MyServiceBus, not with an equivalent serializer of our own: nothing here decides a format,
/// chooses options or fills an envelope.
/// </remarks>
public static class MyServiceBusEventSerialization
{
    private static readonly MethodInfo getMessageBodyMethod = typeof(SendContext).GetMethods()
        .Single(method => method is {Name: nameof(SendContext.GetMessageBody), IsGenericMethodDefinition: true});

    private static readonly MethodInfo tryGetMessageMethod = typeof(IInboundMessage).GetMethods()
        .Single(method => method is
            {Name: nameof(IInboundMessage.TryGetMessage), IsGenericMethodDefinition: true});

    /// <summary>
    /// Reports every event MyServiceBus could not write and read back as a configuration error.
    /// </summary>
    /// <remarks>
    /// Public because a transport package validates with it, not because an application has a reason to call
    /// it.
    /// </remarks>
    /// <param name="eventTypes">The concrete event types the driver knows about.</param>
    /// <param name="configurationPath">The configuration path the errors point at.</param>
    /// <exception cref="ArgumentNullException"><paramref name="eventTypes"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<BootstrapValidationError> Validate(
        IEnumerable<Type> eventTypes, string configurationPath) =>
        EventSerializability.Validate(eventTypes, RoundTrip, configurationPath);

    /// <summary>
    /// Writes an event the way MyServiceBus writes it and reads it back the way MyServiceBus reads it.
    /// </summary>
    /// <remarks>
    /// The envelope is built by <see cref="SendContext.GetMessageBody{T}"/>, which is the same code a real
    /// publish goes through, so every value in it — the identifiers, the message type URNs, the addresses, the
    /// host information and the time — is the one MyServiceBus puts there. What is supplied here is only the
    /// event and its concrete type.
    /// </remarks>
    /// <remarks>
    /// The serializer is the one the bus uses when nothing else is configured, which is what this driver
    /// leaves in place. A receive endpoint given a serializer of its own is not covered.
    /// </remarks>
    /// <remarks>
    /// Both calls are closed with reflection because the MyServiceBus API is generic and the event type is
    /// only known at run time.
    /// </remarks>
    /// <exception cref="UnreadableEventException">MyServiceBus could not read the event back.</exception>
    private static void RoundTrip(object @event, Type eventType)
    {
        var serializerFactory = new EnvelopeSerializerFactory();
        var sendContext = new SendContext([eventType], serializerFactory.CreateSerializer());

        var body = (MessageBody) getMessageBodyMethod
            .MakeGenericMethod(eventType)
            .Invoke(sendContext, [@event])!;

        var inboundMessage = serializerFactory.CreateDeserializer().Deserialize(body, sendContext.Headers);

        var arguments = new object?[] {null};
        bool wasRead = (bool) tryGetMessageMethod
            .MakeGenericMethod(eventType)
            .Invoke(inboundMessage, arguments)!;

        if (!wasRead)
        {
            throw new UnreadableEventException(eventType);
        }
    }
}

/// <summary>
/// Thrown when MyServiceBus writes an event but cannot read it back.
/// </summary>
/// <remarks>
/// MyServiceBus answers whether it could read a message back without saying what stopped it, because the read
/// swallows the failure and returns false, so there is no reason to carry here beyond the event type.
/// </remarks>
/// <param name="eventType">The event type that could not be read back.</param>
public sealed class UnreadableEventException(Type eventType)
    : Exception($"MyServiceBus wrote the event '{eventType.FullName}' into an envelope but could not read it " +
                "back out of it. MyServiceBus reports no reason for it.")
{
    /// <summary>Gets the event type that could not be read back.</summary>
    public Type EventType { get; } = eventType;
}
