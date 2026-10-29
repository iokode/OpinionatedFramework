using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Events;

namespace IOKode.OpinionatedFramework.Tests.Hangfire.Config;

/// <summary>
/// Records what each handler saw. Handlers run in a job on a background server, in a scope of their own, so the
/// observation point has to be static.
/// </summary>
public static class HandledEvents
{
    private static readonly ConcurrentBag<(string Handler, string EventName)> handled = [];
    private static readonly ConcurrentBag<Exhaustion> exhaustions = [];
    private static int recoveringAttempts;
    private static int failingAttempts;

    public static void Record<THandler>(IEvent @event)
    {
        handled.Add((typeof(THandler).Name, EventName.Of(@event.GetType())));
    }

    public static bool WasHandledBy<THandler>() =>
        handled.Any(entry => entry.Handler == typeof(THandler).Name);

    /// <summary>Whether any handler at all ran for that event.</summary>
    public static bool AnyHandlerRanFor(string eventName) =>
        handled.Any(entry => entry.EventName == eventName);

    /// <summary>Counts the attempts of the handler that succeeds on its second try.</summary>
    public static int RecoveringAttempts => recoveringAttempts;

    public static int NextRecoveringAttempt() => Interlocked.Increment(ref recoveringAttempts);

    /// <summary>Counts the attempts of the handler that never succeeds.</summary>
    public static int FailingAttempts => failingAttempts;

    public static int NextFailingAttempt() => Interlocked.Increment(ref failingAttempts);

    /// <summary>Every <see cref="EventHandlerExhausted"/> that reached a handler, reduced to what it said.</summary>
    /// <remarks>
    /// The tests share one server, so an exhaustion is told apart by the order it came from rather than by
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
        Interlocked.Exchange(ref failingAttempts, 0);
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

/// <summary>Fails once so the declared retry is what makes it succeed.</summary>
public class FailOnFirstAttempt : IEventHandler<OrderSubmitted>
{
    public Task HandleAsync(OrderSubmitted @event, CancellationToken cancellationToken)
    {
        if (HandledEvents.NextRecoveringAttempt() == 1)
        {
            throw new InvalidOperationException("Simulated failure on the first attempt.");
        }

        HandledEvents.Record<FailOnFirstAttempt>(@event);
        return Task.CompletedTask;
    }
}

/// <summary>Never succeeds, so it exhausts exactly the attempts its policy declares.</summary>
public class AlwaysFail : IEventHandler<OrderCancelled>
{
    public Task HandleAsync(OrderCancelled @event, CancellationToken cancellationToken)
    {
        HandledEvents.NextFailingAttempt();
        throw new InvalidOperationException("This handler always fails.");
    }
}

/// <summary>What an <see cref="EventHandlerExhausted"/> said, read while its JSON was still at hand.</summary>
public sealed record Exhaustion(string EventName, string HandlerName, int Attempts, Guid? OrderId);

/// <summary>Fails without a single retry, so its first failure is already its last.</summary>
public class AlwaysFailWithoutRetry : IEventHandler<OrderRejected>
{
    public Task HandleAsync(OrderRejected @event, CancellationToken cancellationToken)
    {
        // Recorded before failing, so a test can tell that it ran when nothing else follows from it.
        HandledEvents.Record<AlwaysFailWithoutRetry>(@event);
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
        HandledEvents.Record<FailOnExhaustion>(@event);
        throw new InvalidOperationException("This handler of the exhaustion event always fails.");
    }
}
