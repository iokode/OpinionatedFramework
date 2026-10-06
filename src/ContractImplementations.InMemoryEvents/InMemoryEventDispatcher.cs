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
/// it has already moved on. Failures are logged instead, and they neither stop nor hide the other handlers.
/// </remarks>
/// <remarks>
/// Each execution gets an independent service scope. It deliberately does not join the scope of whoever raised
/// the event, because that scope is typically disposed while the handler is still running.
/// </remarks>
/// <remarks>
/// This dispatcher offers no durability: work that has not finished is lost if the process stops. Disposal
/// drains what is queued, which covers an orderly shutdown but not a crash.
/// </remarks>
public sealed class InMemoryEventDispatcher : IEventDispatcher, IAsyncDisposable
{
    private readonly InMemoryEventsOptions options;
    private readonly Channel<PendingExecution> queue = Channel.CreateUnbounded<PendingExecution>();
    private readonly ConcurrentDictionary<Type, SemaphoreSlim> concurrencyGates = new();
    private readonly CancellationTokenSource stopping = new();
    private readonly Task[] workers;
    private int disposed;

    /// <summary>Creates the dispatcher and starts its workers.</summary>
    /// <param name="options">The declared handlers and worker count.</param>
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
        // so a missing name is found with the in-memory driver too.
        _ = EventName.Of(@event.GetType());

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
        }
        finally
        {
            gate?.Release();
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

    private sealed record PendingExecution(
        EventHandlerRegistration<InMemoryEventHandlerPolicy> Registration,
        IEvent Event);
}
