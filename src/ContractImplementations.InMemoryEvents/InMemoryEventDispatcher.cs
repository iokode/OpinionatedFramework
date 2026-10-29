using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Events;
using IOKode.OpinionatedFramework.Internals.Events;
using IOKode.OpinionatedFramework.Logging;
using IOKode.OpinionatedFramework.ServiceContainer;
using IOKode.OpinionatedFramework.ServiceLocation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IOKode.OpinionatedFramework.ContractImplementations.InMemoryEvents;

/// <summary>
/// Dispatches events to handlers running on background workers of the current process.
/// </summary>
/// <remarks>
/// Dispatching only queues the work, so raising an event never delays whoever raised it. Handlers run afterwards
/// on a worker of this dispatcher, which is why a failure cannot be reported back to the code that dispatched:
/// it has already moved on. Failures are logged instead, and they neither stop nor hide the other handlers. A
/// handler that fails its last attempt also raises <see cref="EventHandlerExhausted"/>, when the application
/// declared it.
/// </remarks>
/// <remarks>
/// Each execution gets an independent service scope. It deliberately does not join the scope of whoever raised
/// the event, because that scope is typically disposed while the handler is still running.
/// </remarks>
/// <remarks>
/// This dispatcher offers no durability: work that has not finished is lost if the process stops. Disposal
/// drains what is queued, which covers an orderly shutdown but not a crash.
/// </remarks>
public class InMemoryEventDispatcher : IEventDispatcher, IAsyncDisposable
{
    private readonly InMemoryEventsOptions options;
    private readonly Channel<PendingExecution> queue = Channel.CreateUnbounded<PendingExecution>();
    private readonly ConcurrentDictionary<Type, SemaphoreSlim> concurrencyGates = new();
    private readonly CancellationTokenSource stopping = new();
    private readonly Task[] workers;
    private int disposed;

    /// <summary>Creates the dispatcher and starts its workers.</summary>
    /// <param name="options">The event declarations and the worker count.</param>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public InMemoryEventDispatcher(InMemoryEventsOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        this.options = options;
        this.workers = Enumerable.Range(0, options.WorkerCount)
            .Select(_ => Task.Run(DrainAsync))
            .ToArray();
    }

    /// <inheritdoc/>
    public Task DispatchAsync(IPublishableEvent @event, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(@event);

        // Fails here rather than in a driver that would only notice when the event has to leave the process,
        // so an event the application never declared it raises is refused with the in-memory driver too.
        this.options.EnsureDeclaredAsPublished(@event.GetType());

        foreach (var registration in this.options.GetRegistrationsFor(@event.GetType()))
        {
            // Unbounded, so this never blocks the caller.
            this.queue.Writer.TryWrite(new PendingExecution(registration, @event));
        }

        return Task.CompletedTask;
    }

    /// <summary>Stops accepting work, drains what is queued, and stops the workers.</summary>
    /// <remarks>Safe to call more than once.</remarks>
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref this.disposed, 1) == 1)
        {
            return;
        }

        this.queue.Writer.TryComplete();

        try
        {
            await Task.WhenAll(this.workers);
        }
        finally
        {
            await this.stopping.CancelAsync();
            this.stopping.Dispose();

            foreach (var gate in this.concurrencyGates.Values)
            {
                gate.Dispose();
            }
        }
    }

    private async Task DrainAsync()
    {
        while (await this.queue.Reader.WaitToReadAsync())
        {
            while (this.queue.Reader.TryRead(out var pending))
            {
                await ExecuteAsync(pending);
            }
        }
    }

    private async Task ExecuteAsync(PendingExecution pending)
    {
        var gate = pending.Registration.Policy.ConcurrencyLimit is { } limit
            ? this.concurrencyGates.GetOrAdd(pending.Registration.HandlerType, _ => new SemaphoreSlim(limit, limit))
            : null;

        if (gate is not null)
        {
            await gate.WaitAsync(this.stopping.Token);
        }

        try
        {
            await RunWithRetryAsync(pending);
        }
        catch (Exception exception)
        {
            Log(pending, exception);

            // The retry loop only lets a failure out once the policy has no attempts left. Nothing in flight is
            // cancelled: disposing waits for the workers to drain before it cancels the stopping token.
            await RaiseExhaustedAsync(pending, pending.Registration.Policy.RetryCount + 1);
        }
        finally
        {
            gate?.Release();
        }
    }

    private async Task RaiseExhaustedAsync(PendingExecution pending, int attempts)
    {
        var eventType = pending.Event.GetType();
        var eventName = EventName.Of(eventType);

        if (!EventHandlerExhaustion.IsRaisedFor(this.options, eventName))
        {
            return;
        }

        try
        {
            var exhausted = EventHandlerExhaustion.Describe(
                eventName,
                EventSerializer.Serialize(pending.Event, eventType),
                pending.Registration.HandlerType.FullName!,
                attempts);

            await DispatchAsync(exhausted, CancellationToken.None);
        }
        catch (Exception exception)
        {
            // Nothing can be reported back to whoever dispatched, and a worker that let this escape would stop.
            LogNotRaised(pending, exception);
        }
    }

    private async Task RunWithRetryAsync(PendingExecution pending)
    {
        var policy = pending.Registration.Policy;
        var attemptsLeft = policy.RetryCount;

        while (true)
        {
            try
            {
                await RunOnceAsync(pending);
                return;
            }
            catch (Exception) when (attemptsLeft > 0)
            {
                attemptsLeft--;

                if (policy.RetryDelay is { } delay)
                {
                    await Task.Delay(delay, this.stopping.Token);
                }
            }
        }
    }

    private async Task RunOnceAsync(PendingExecution pending)
    {
        // An independent scope, because the handler begins an operation whose lifetime is its own.
        await using var scope = Container.Advanced.CreateIndependentScope();
        var serviceProvider = Locator.ServiceProvider!;

        await pending.Registration.Invoke(serviceProvider, pending.Event, this.stopping.Token);
    }

    private static void Log(PendingExecution pending, Exception exception)
    {
        // Best effort: an application without a logging driver still gets a dispatcher that keeps working.
        var logger = Locator.ServiceProvider?.GetService<ILogging>()?.FromCategory<InMemoryEventDispatcher>();

        logger?.LogError(
            exception,
            "The handler {Handler} failed while handling the event {Event}.",
            pending.Registration.HandlerType.Name,
            EventName.Of(pending.Event.GetType()));
    }

    private static void LogNotRaised(PendingExecution pending, Exception exception)
    {
        var logger = Locator.ServiceProvider?.GetService<ILogging>()?.FromCategory<InMemoryEventDispatcher>();

        logger?.LogError(
            exception,
            "The handler {Handler} exhausted its attempts on the event {Event}, and raising {Exhausted} failed.",
            pending.Registration.HandlerType.Name,
            EventName.Of(pending.Event.GetType()),
            EventName.Of<EventHandlerExhausted>());
    }

    private sealed record PendingExecution(
        EventHandlerRegistration<InMemoryEventHandlerPolicy> Registration,
        IEvent Event);
}
