using System;

namespace IOKode.OpinionatedFramework.ContractImplementations.MyServiceBus;

/// <summary>
/// Declares how the broker-backed driver executes a handler.
/// </summary>
/// <remarks>
/// The policy maps onto the transport's own machinery: retry is the receive pipeline's retry filter, and the
/// concurrency limit is the receive endpoint's. A failure that survives the retries is therefore a real delivery
/// failure, so the original message reaches the endpoint's error queue and a fault is published, which is what
/// makes the attempt visible to whoever monitors the bus.
/// </remarks>
public sealed class MyServiceBusEventHandlerPolicy
{
    /// <summary>Gets how many times the receive pipeline retries the handler after a failed attempt.</summary>
    public int RetryCount { get; private set; }

    /// <summary>Gets the delay between attempts, or <see langword="null"/> to retry immediately.</summary>
    public TimeSpan? RetryDelay { get; private set; }

    /// <summary>Gets how many deliveries the handler's endpoint processes at once, per subscribed process.</summary>
    public int? ConcurrencyLimit { get; private set; }

    /// <summary>Gets whether the application declared a retry policy for the handler.</summary>
    public bool IsRetryConfigured { get; private set; }

    /// <summary>
    /// Retries the handler after a failed attempt, before the delivery is treated as failed.
    /// </summary>
    /// <param name="count">How many times the handler is retried. Must not be negative.</param>
    /// <param name="delay">The delay between attempts, or <see langword="null"/> to retry immediately.</param>
    /// <returns>The same policy, to allow chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> or <paramref name="delay"/> is negative.</exception>
    public MyServiceBusEventHandlerPolicy Retry(int count, TimeSpan? delay = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        if (delay is not null)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(delay.Value, TimeSpan.Zero, nameof(delay));
        }

        RetryCount = count;
        RetryDelay = delay;
        IsRetryConfigured = true;
        return this;
    }

    /// <summary>
    /// Bounds how many deliveries the handler's endpoint processes at once.
    /// </summary>
    /// <remarks>
    /// The limit applies to each subscribed process, so several replicas of the same subscriber multiply it.
    /// </remarks>
    /// <param name="limit">The maximum number of concurrent deliveries. Must be greater than zero.</param>
    /// <returns>The same policy, to allow chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="limit"/> is not greater than zero.</exception>
    public MyServiceBusEventHandlerPolicy Concurrency(int limit)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(limit, 1);

        ConcurrencyLimit = limit;
        return this;
    }
}
