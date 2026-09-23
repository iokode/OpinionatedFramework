using System;
using IOKode.OpinionatedFramework.Events;

namespace IOKode.OpinionatedFramework.Tests.InMemoryEvents.Config;

/// <summary>Raised here and reacted to here: the symmetric case.</summary>
[EventName("tests.order-submitted")]
public class OrderSubmitted : IPublishableEvent, ISubscribableEvent
{
    public required Guid OrderId { get; init; }
    public required string Customer { get; init; }
}

/// <summary>Also both directions, used for the failure and retry cases.</summary>
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

/// <summary>Raised by another application; this one only reacts. It cannot be dispatched from here.</summary>
[EventName("tests.partner-payment")]
public class PartnerPayment : ISubscribableEvent
{
    public required Guid OrderId { get; init; }
}

/// <summary>An event with no name, used to prove the attribute is enforced.</summary>
public class UnnamedEvent : IPublishableEvent;

/// <summary>Declares the same name as <see cref="OrderSubmitted"/>, which no driver may accept.</summary>
[EventName("tests.order-submitted")]
public class DuplicateOrderSubmitted : ISubscribableEvent
{
    public required Guid OrderId { get; init; }
}
