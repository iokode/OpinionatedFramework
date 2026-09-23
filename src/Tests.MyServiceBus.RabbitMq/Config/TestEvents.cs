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
/// Reacted to only through the handler registered against the event interface. Nothing reveals it to the
/// driver, so it is the event that has to be declared with <c>AddEvent</c>.
/// </summary>
[EventName("tests.inventory-adjusted")]
public class InventoryAdjusted : IPublishableEvent, ISubscribableEvent
{
    public required Guid OrderId { get; init; }
}
