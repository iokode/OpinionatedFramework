using System;
using IOKode.OpinionatedFramework.Events;

namespace IOKode.OpinionatedFramework.Tests.Hangfire.Config;

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
