using System;

namespace IOKode.OpinionatedFramework.ContractImplementations.InMemoryEvents;

/// <summary>
/// Declares how the in-memory dispatcher executes a handler.
/// </summary>
/// <remarks>
/// This driver owns its execution loop, so it implements the policy itself. It offers no timeout, because a
/// deadline that only some drivers can honor would change meaning when the driver changes.
/// </remarks>
public sealed class InMemoryEventHandlerPolicy
{
    /// <summary>Gets how many times the handler is retried after a failed attempt.</summary>
    public int RetryCount { get; private set; }

    /// <summary>Gets the delay between attempts, or <see langword="null"/> to retry immediately.</summary>
    public TimeSpan? RetryDelay { get; private set; }

    /// <summary>Gets how many executions of this handler may run at once in this process.</summary>
    public int? ConcurrencyLimit { get; private set; }

    /// <summary>
    /// Retries the handler after a failed attempt.
    /// </summary>
    /// <param name="count">How many times the handler is retried. Must not be negative.</param>
    /// <param name="delay">The delay between attempts, or <see langword="null"/> to retry immediately.</param>
    /// <returns>The same policy, to allow chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> or <paramref name="delay"/> is negative.</exception>
    public InMemoryEventHandlerPolicy Retry(int count, TimeSpan? delay = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        if (delay is not null)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(delay.Value, TimeSpan.Zero, nameof(delay));
        }

        RetryCount = count;
        RetryDelay = delay;
        return this;
    }

    /// <summary>
    /// Bounds how many executions of this handler run at once in this process.
    /// </summary>
    /// <param name="limit">The maximum number of concurrent executions. Must be greater than zero.</param>
    /// <returns>The same policy, to allow chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="limit"/> is not greater than zero.</exception>
    public InMemoryEventHandlerPolicy Concurrency(int limit)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(limit, 1);

        ConcurrencyLimit = limit;
        return this;
    }
}
