using System;

namespace IOKode.OpinionatedFramework.ContractImplementations.Hangfire;

/// <summary>
/// Declares how Hangfire executes a handler.
/// </summary>
/// <remarks>
/// The policy maps onto Hangfire's own retry machinery, so an attempt appears in the dashboard as a Hangfire
/// retry rather than being hidden inside the job.
/// </remarks>
/// <remarks>
/// There is no concurrency setting: Hangfire bounds concurrency per server and queue, and its open-source
/// edition can only serialize a job type completely, not run a chosen number of them at once. Declaring a limit
/// that the driver would have to emulate is exactly what this design avoids.
/// </remarks>
public sealed class HangfireEventHandlerPolicy
{
    /// <summary>Gets how many times Hangfire retries the handler after a failed attempt.</summary>
    public int RetryCount { get; private set; }

    /// <summary>Gets the delay between attempts, or <see langword="null"/> for Hangfire's own backoff.</summary>
    public TimeSpan? RetryDelay { get; private set; }

    /// <summary>Gets whether the application declared a retry policy for the handler.</summary>
    public bool IsRetryConfigured { get; private set; }

    /// <summary>
    /// Retries the handler after a failed attempt.
    /// </summary>
    /// <remarks>
    /// Hangfire schedules retries with a whole number of seconds, so a delay under one second retries at once.
    /// </remarks>
    /// <param name="count">How many times the handler is retried. Must not be negative.</param>
    /// <param name="delay">The delay between attempts, or <see langword="null"/> for Hangfire's own backoff.</param>
    /// <returns>The same policy, to allow chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> or <paramref name="delay"/> is negative.</exception>
    public HangfireEventHandlerPolicy Retry(int count, TimeSpan? delay = null)
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
}
