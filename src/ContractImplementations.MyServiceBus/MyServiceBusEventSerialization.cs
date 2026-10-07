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
    private static readonly MethodInfo roundTripMethod = typeof(MyServiceBusEventSerialization)
        .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
        .Single(method => method is {Name: nameof(RoundTrip), IsGenericMethodDefinition: true});

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
    /// The round trip is closed over the event type with a delegate rather than invoked through reflection, so
    /// what MyServiceBus throws when it cannot read the event back arrives at the caller as it was thrown.
    /// </remarks>
    private static void RoundTrip(object @event, Type eventType) =>
        roundTripMethod.MakeGenericMethod(eventType).CreateDelegate<Action<object>>().Invoke(@event);

    /// <summary>
    /// Writes an event of a known type into an envelope and reads it back out of it.
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
    /// The answer of the read is of no interest: MyServiceBus answers <see langword="false"/> only for a
    /// payload it has already read as another type, and reports every failure by throwing.
    /// </remarks>
    /// <exception cref="MessageDeserializationException">MyServiceBus could not read the event back.</exception>
    private static void RoundTrip<TEvent>(object @event) where TEvent : class
    {
        var serializerFactory = new EnvelopeSerializerFactory();
        var sendContext = new SendContext([typeof(TEvent)], serializerFactory.CreateSerializer());

        var body = sendContext.GetMessageBody((TEvent) @event);
        var inboundMessage = serializerFactory.CreateDeserializer().Deserialize(body, sendContext.Headers);

        _ = inboundMessage.TryGetMessage<TEvent>(out _);
    }
}
