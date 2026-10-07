using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using MyServiceBus;

namespace IOKode.OpinionatedFramework.Tests.MyServiceBus.RabbitMq.Config.PartnerApplication;

/// <summary>
/// The contract the other application declares for the event named <c>tests.partner-payment</c>.
/// </summary>
/// <remarks>
/// Its own CLR type, in its own namespace, implementing nothing of the framework: what it shares with the
/// application under test is the declared event name and nothing else, which is exactly what the wire identity
/// has to carry for the two to meet.
/// </remarks>
public class PartnerPayment
{
    public Guid OrderId { get; set; }
}

/// <summary>
/// The contract the other application declares for the event named <c>tests.order-submitted</c>.
/// </summary>
public class OrderSubmitted
{
    public Guid OrderId { get; set; }

    public string Customer { get; set; } = string.Empty;
}

/// <summary>
/// Records what the other application received.
/// </summary>
public static class PartnerSubscriber
{
    /// <summary>The queue of the other application, which the framework knows nothing about.</summary>
    public const string EndpointName = "tests--partner-order-observer";

    private static readonly ConcurrentBag<Guid> received = [];

    public static void Record(Guid orderId) => received.Add(orderId);

    public static bool Received(Guid orderId) => received.Any(id => id == orderId);

    public static void Reset() => received.Clear();
}

/// <summary>
/// Reacts to the event the application under test raises, without sharing its type.
/// </summary>
/// <remarks>
/// A plain MyServiceBus consumer rather than an <see cref="Events.IEventHandler{TEvent}"/>, because this is
/// another application and nothing of the framework reaches it.
/// </remarks>
public class PartnerOrderConsumer : IConsumer<OrderSubmitted>
{
    public Task Consume(ConsumeContext<OrderSubmitted> context)
    {
        PartnerSubscriber.Record(context.Message.OrderId);
        return Task.CompletedTask;
    }
}
