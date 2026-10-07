using System;

namespace IOKode.OpinionatedFramework.ContractImplementations.MyServiceBus;

/// <summary>
/// One handler subscribed to one concrete event type.
/// </summary>
/// <remarks>
/// Computed here rather than by each transport, because deciding which queue carries which event to which
/// handler is the same problem whichever broker is underneath.
/// </remarks>
/// <param name="EndpointName">The queue carrying this event to this handler.</param>
/// <param name="EventType">The concrete event type delivered to the consumer.</param>
/// <param name="HandlerType">The handler type.</param>
/// <param name="ConsumerType">The closed consumer type adapting the handler to the transport.</param>
/// <param name="Policy">How the transport is asked to execute the handler.</param>
public sealed record EventSubscription(
    string EndpointName,
    Type EventType,
    Type HandlerType,
    Type ConsumerType,
    MyServiceBusEventHandlerPolicy Policy);
