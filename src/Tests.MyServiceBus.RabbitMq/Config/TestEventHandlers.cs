using System;
using System.Collections.Concurrent;
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

    public static void Reset()
    {
        handled.Clear();
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
/// Registered against the event interface, which is how an application observes or stores everything it reacts
/// to.
/// </summary>
public class StoreEvent : IEventHandler<ISubscribableEvent>
{
    public Task HandleAsync(ISubscribableEvent @event, CancellationToken cancellationToken)
    {
        // Registered against the interface, so it reaches the identifier through the concrete event it received.
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
