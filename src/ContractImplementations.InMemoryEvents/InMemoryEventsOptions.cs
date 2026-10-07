using System;
using IOKode.OpinionatedFramework.Events;

namespace IOKode.OpinionatedFramework.ContractImplementations.InMemoryEvents;

/// <summary>
/// Configures the in-memory event dispatcher.
/// </summary>
/// <remarks>
/// The handlers and the events this application raises are declared here rather than in a registry the
/// dispatcher consults while publishing, so the driver knows the complete set before the first event is
/// dispatched.
/// </remarks>
public sealed class InMemoryEventsOptions : EventDeclarations<InMemoryEventHandlerPolicy>
{
    /// <summary>Gets how many handler executions the dispatcher runs at once across every handler.</summary>
    public int WorkerCount { get; private set; } = Environment.ProcessorCount;

    /// <summary>
    /// Sets how many handler executions the dispatcher runs at once across every handler.
    /// </summary>
    /// <param name="workerCount">The number of workers draining the queue. Must be greater than zero.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="workerCount"/> is not greater than zero.</exception>
    public void SetWorkerCount(int workerCount)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(workerCount, 1);

        WorkerCount = workerCount;
    }
}
