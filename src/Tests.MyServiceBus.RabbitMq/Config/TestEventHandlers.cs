using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Events;

namespace IOKode.OpinionatedFramework.Tests.MyServiceBus.RabbitMq.Config;

/// <summary>
/// Records what each handler saw. Handlers run when the broker delivers to their queue, in a scope of their
/// own, so the observation point has to be static.
/// </summary>
public static class HandledEvents
{
    private static readonly ConcurrentBag<(string Handler, Guid EventId)> handled = [];
    private static readonly ConcurrentBag<Exhaustion> exhaustions = [];
    private static int recoveringAttempts;

    public static void Record<THandler>(Guid eventId)
    {
        handled.Add((typeof(THandler).Name, eventId));
    }

    /// <summary>
    /// Whether the handler saw that exact event instance.
    /// </summary>
    /// <remarks>
    /// Identity rather than event type, because the tests of this class share one fixture and one broker: an
    /// assertion on the type alone could be satisfied by a dispatch another test made.
    /// </remarks>
    public static bool WasHandledBy<THandler>(Guid eventId) =>
        handled.Any(entry => entry.Handler == typeof(THandler).Name && entry.EventId == eventId);

    /// <summary>Counts the attempts of the handler that succeeds on its second delivery.</summary>
    public static int RecoveringAttempts => recoveringAttempts;

    public static int NextRecoveringAttempt() => Interlocked.Increment(ref recoveringAttempts);

    /// <summary>Every <see cref="EventHandlerExhausted"/> that reached a handler, reduced to what it said.</summary>
    /// <remarks>
    /// The tests share one broker, so an exhaustion is told apart by the order it came from rather than by
    /// counting them all.
    /// </remarks>
    public static IReadOnlyCollection<Exhaustion> Exhaustions => exhaustions;

    public static void RecordExhaustion(EventHandlerExhausted @event)
    {
        exhaustions.Add(new Exhaustion(
            @event.EventName,
            @event.HandlerName,
            @event.Attempts,
            @event.EventBody.RootElement.TryGetProperty("orderId", out var orderId) ? orderId.GetGuid() : null));
    }

    public static void Reset()
    {
        handled.Clear();
        exhaustions.Clear();
        Interlocked.Exchange(ref recoveringAttempts, 0);
    }
}

public class SendConfirmationEmail : IEventHandler<OrderSubmitted>
{
    public Task HandleAsync(OrderSubmitted @event, CancellationToken cancellationToken)
    {
        HandledEvents.Record<SendConfirmationEmail>(@event.OrderId);
        return Task.CompletedTask;
    }
}

public class UpdateStatistics : IEventHandler<OrderSubmitted>
{
    public Task HandleAsync(OrderSubmitted @event, CancellationToken cancellationToken)
    {
        HandledEvents.Record<UpdateStatistics>(@event.OrderId);
        return Task.CompletedTask;
    }
}

/// <summary>
/// Written against the event interface, which is how an application stores or audits what it reacts to. It is
/// declared once per event it covers, and the contravariance of the handler contract is what makes it
/// acceptable there.
/// </summary>
public class StoreEvent : IEventHandler<ISubscribableEvent>
{
    public Task HandleAsync(ISubscribableEvent @event, CancellationToken cancellationToken)
    {
        // Written against the interface, so it reaches the identifier through the concrete event it received.
        Guid eventId = @event switch
        {
            OrderSubmitted submitted => submitted.OrderId,
            OrderCancelled cancelled => cancelled.OrderId,
            PartnerPayment payment => payment.OrderId,
            InventoryAdjusted adjusted => adjusted.OrderId,
            _ => Guid.Empty
        };

        HandledEvents.Record<StoreEvent>(eventId);
        return Task.CompletedTask;
    }
}

/// <summary>Reacts to an event this application never raises.</summary>
public class HandlePartnerPayment : IEventHandler<PartnerPayment>
{
    public Task HandleAsync(PartnerPayment @event, CancellationToken cancellationToken)
    {
        HandledEvents.Record<HandlePartnerPayment>(@event.OrderId);
        return Task.CompletedTask;
    }
}

/// <summary>Fails the first delivery, so the endpoint's retry is what makes it succeed.</summary>
public class FailOnFirstDelivery : IEventHandler<OrderCancelled>
{
    public Task HandleAsync(OrderCancelled @event, CancellationToken cancellationToken)
    {
        if (HandledEvents.NextRecoveringAttempt() == 1)
        {
            throw new InvalidOperationException("Simulated failure on the first delivery.");
        }

        HandledEvents.Record<FailOnFirstDelivery>(@event.OrderId);
        return Task.CompletedTask;
    }
}

/// <summary>What an <see cref="EventHandlerExhausted"/> said, read while its JSON was still at hand.</summary>
public sealed record Exhaustion(string EventName, string HandlerName, int Attempts, Guid? OrderId);

/// <summary>Never succeeds, so it exhausts exactly the attempts its policy declares.</summary>
public class AlwaysFail : IEventHandler<OrderRejected>
{
    public Task HandleAsync(OrderRejected @event, CancellationToken cancellationToken)
    {
        throw new InvalidOperationException("This handler always fails.");
    }
}

/// <summary>Fails without a retry policy, so its first failure is already its last.</summary>
public class AlwaysFailWithoutRetry : IEventHandler<OrderRejected>
{
    public Task HandleAsync(OrderRejected @event, CancellationToken cancellationToken)
    {
        // Recorded before failing, so a test can tell that it ran when nothing else follows from it.
        HandledEvents.Record<AlwaysFailWithoutRetry>(@event.OrderId);
        throw new InvalidOperationException("This handler always fails, and is never retried.");
    }
}

public class RecordExhaustion : IEventHandler<EventHandlerExhausted>
{
    public Task HandleAsync(EventHandlerExhausted @event, CancellationToken cancellationToken)
    {
        HandledEvents.RecordExhaustion(@event);
        return Task.CompletedTask;
    }
}

/// <summary>A handler of the exhaustion event that fails, which must not raise the event again.</summary>
public class FailOnExhaustion : IEventHandler<EventHandlerExhausted>
{
    public Task HandleAsync(EventHandlerExhausted @event, CancellationToken cancellationToken)
    {
        HandledEvents.Record<FailOnExhaustion>(Guid.Empty);
        throw new InvalidOperationException("This handler of the exhaustion event always fails.");
    }
}
