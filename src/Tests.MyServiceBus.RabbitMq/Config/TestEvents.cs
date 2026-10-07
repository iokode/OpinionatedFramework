using System;
using IOKode.OpinionatedFramework.Events;

namespace IOKode.OpinionatedFramework.Tests.MyServiceBus.RabbitMq.Config;

[EventName("tests.order-submitted")]
public class OrderSubmitted : IPublishableEvent, ISubscribableEvent
{
    public required Guid OrderId { get; init; }
    public required string Customer { get; init; }
}

[EventName("tests.order-cancelled")]
public class OrderCancelled : IPublishableEvent, ISubscribableEvent
{
    public required Guid OrderId { get; init; }
}

/// <summary>Raised here and nobody here reacts to it. No handler can be registered for it.</summary>
[EventName("tests.audit-recorded")]
public class AuditRecorded : IPublishableEvent
{
    public required Guid OrderId { get; init; }
}

/// <summary>
/// Raised by another application; this one only reacts. It cannot be handed to the dispatcher from here, which
/// is why the test that exercises it publishes through the transport directly.
/// </summary>
[EventName("tests.partner-payment")]
public class PartnerPayment : ISubscribableEvent
{
    public required Guid OrderId { get; init; }
}

/// <summary>
/// The only handler it is declared for is the one written against the event interface, so it is the event whose
/// queue proves a handler of that shape is subscribed to a concrete event like any other.
/// </summary>
[EventName("tests.inventory-adjusted")]
public class InventoryAdjusted : IPublishableEvent, ISubscribableEvent
{
    public required Guid OrderId { get; init; }
}

/// <summary>
/// An event a driver cannot read back: the serializer has no way to choose between its two constructors.
/// </summary>
/// <remarks>
/// Declared here rather than in a fixture because a driver rejects it while validating, so no application
/// using it ever starts.
/// </remarks>
[EventName("tests.not-readable")]
public class NotReadableEvent : IPublishableEvent, ISubscribableEvent
{
    public NotReadableEvent(Guid orderId)
    {
        this.OrderId = orderId;
    }

    public NotReadableEvent(Guid orderId, string reason) : this(orderId)
    {
        this.Reason = reason;
    }

    public Guid OrderId { get; }

    public string? Reason { get; }
}
