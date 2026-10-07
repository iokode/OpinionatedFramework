using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Events;

namespace IOKode.OpinionatedFramework.Tests.InMemoryEvents.Config;

/// <summary>
/// Records what each handler saw. Handlers are constructed by the container, and under the broker driver they
/// run in another scope entirely, so the observation point has to be static.
/// </summary>
public static class HandledEvents
{
    private static readonly ConcurrentBag<(string Handler, string EventName, string? Payload)> handled = [];
    private static int attempts;

    /// <param name="event">The event the handler received.</param>
    /// <param name="payload">
    /// Something the handler read out of it, for an assertion that the data arrived and not only the event.
    /// </param>
    public static void Record<THandler>(IEvent @event, string? payload = null)
    {
        handled.Add((typeof(THandler).Name, EventName.Of(@event.GetType()), payload));
    }

    public static int CountFor<THandler>() =>
        handled.Count(entry => entry.Handler == typeof(THandler).Name);

    public static bool WasHandledBy<THandler>() => CountFor<THandler>() > 0;

    /// <summary>Whether the handler saw that event carrying that value.</summary>
    public static bool SawPayload<THandler>(string eventName, string payload) =>
        handled.Any(entry => entry.Handler == typeof(THandler).Name
                             && entry.EventName == eventName
                             && entry.Payload == payload);

    public static int Attempts => attempts;

    public static int NextAttempt() => Interlocked.Increment(ref attempts);

    private static int running;
    private static int maxRunning;

    /// <summary>The highest number of handler executions observed running at the same time.</summary>
    public static int MaxConcurrent => maxRunning;

    public static void EnterConcurrentSection()
    {
        int current = Interlocked.Increment(ref running);

        int observed;
        do
        {
            observed = maxRunning;
            if (current <= observed)
            {
                return;
            }
        }
        while (Interlocked.CompareExchange(ref maxRunning, current, observed) != observed);
    }

    public static void LeaveConcurrentSection() => Interlocked.Decrement(ref running);

    public static void Reset()
    {
        handled.Clear();
        Interlocked.Exchange(ref attempts, 0);
        Interlocked.Exchange(ref running, 0);
        Interlocked.Exchange(ref maxRunning, 0);
    }
}

public class SendConfirmationEmail : IEventHandler<OrderSubmitted>
{
    public Task HandleAsync(OrderSubmitted @event, CancellationToken cancellationToken)
    {
        HandledEvents.Record<SendConfirmationEmail>(@event);
        return Task.CompletedTask;
    }
}

public class UpdateStatistics : IEventHandler<OrderSubmitted>
{
    public Task HandleAsync(OrderSubmitted @event, CancellationToken cancellationToken)
    {
        HandledEvents.Record<UpdateStatistics>(@event);
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
        // What arrives is the concrete event, so the data it declares is reachable from here.
        HandledEvents.Record<StoreEvent>(@event, (@event as OrderSubmitted)?.Customer);
        return Task.CompletedTask;
    }
}

/// <summary>Reacts to an event this application never raises.</summary>
public class HandlePartnerPayment : IEventHandler<PartnerPayment>
{
    public Task HandleAsync(PartnerPayment @event, CancellationToken cancellationToken)
    {
        HandledEvents.Record<HandlePartnerPayment>(@event);
        return Task.CompletedTask;
    }
}

/// <summary>Exists so a name clash can be produced from two concrete handler registrations.</summary>
public class HandleDuplicate : IEventHandler<DuplicateOrderSubmitted>
{
    public Task HandleAsync(DuplicateOrderSubmitted @event, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

public class FailOnFirstAttempt : IEventHandler<OrderSubmitted>
{
    public Task HandleAsync(OrderSubmitted @event, CancellationToken cancellationToken)
    {
        if (HandledEvents.NextAttempt() == 1)
        {
            throw new InvalidOperationException("Simulated failure on the first attempt.");
        }

        HandledEvents.Record<FailOnFirstAttempt>(@event);
        return Task.CompletedTask;
    }
}

public class AlwaysFail : IEventHandler<OrderCancelled>
{
    public Task HandleAsync(OrderCancelled @event, CancellationToken cancellationToken)
    {
        throw new InvalidOperationException("This handler always fails.");
    }
}

public class RecordCancellation : IEventHandler<OrderCancelled>
{
    public Task HandleAsync(OrderCancelled @event, CancellationToken cancellationToken)
    {
        HandledEvents.Record<RecordCancellation>(@event);
        return Task.CompletedTask;
    }
}

/// <summary>Takes long enough that a synchronous dispatcher would be caught waiting for it.</summary>
public class SlowHandler : IEventHandler<OrderSubmitted>
{
    public async Task HandleAsync(OrderSubmitted @event, CancellationToken cancellationToken)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(300), cancellationToken);
        HandledEvents.Record<SlowHandler>(@event);
    }
}

/// <summary>Reports how many executions of it overlap, so the worker count can be observed.</summary>
public class ConcurrencyProbe : IEventHandler<OrderSubmitted>
{
    public async Task HandleAsync(OrderSubmitted @event, CancellationToken cancellationToken)
    {
        HandledEvents.EnterConcurrentSection();
        try
        {
            await Task.Delay(TimeSpan.FromMilliseconds(200), cancellationToken);
        }
        finally
        {
            HandledEvents.LeaveConcurrentSection();
        }

        HandledEvents.Record<ConcurrencyProbe>(@event);
    }
}
