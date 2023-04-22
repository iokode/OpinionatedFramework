using System;
using System.Threading.Tasks;

namespace IOKode.OpinionatedFramework.Foundation.Utilities;

/// <summary>
/// Provides a set of static methods for polling a function until a specific condition is met or a timeout occurs.
/// </summary>
/// <remarks>
/// The methods in this class can be used to repeatedly execute a given function asynchronously, waiting for
/// the function to return 'true'. Execution will continue either until the function returns 'true', or until
/// a specified timeout duration has been reached.
/// </remarks>
public static class PollingUtility
{
    /// <summary>
    /// Polls the specified synchronous function until it returns true or the timeout duration has been reached.
    /// </summary>
    /// <param name="function">The synchronous function to poll.</param>
    /// <param name="timeout">The duration after which the polling will stop if the function does not return true.</param>
    /// <param name="pollingInterval">The duration to wait between polling attempts.</param>
    /// <returns>A ValueTask that represents the asynchronous operation. The task result is true if the function returns true before the timeout duration, otherwise false.</returns>
    public static async ValueTask<bool> WaitUntilTrueAsync(Func<bool> function, TimeSpan timeout,
        TimeSpan pollingInterval)
    {
        var startTime = DateTime.UtcNow;

        while (DateTime.UtcNow - startTime < timeout)
        {
            if (function())
            {
                return true;
            }

            await Task.Delay(pollingInterval);
        }

        return false;
    }

    /// <summary>
    /// Polls the specified asynchronous function until it returns true or the timeout duration has been reached.
    /// </summary>
    /// <param name="function">The asynchronous function to poll.</param>
    /// <param name="timeout">The duration after which the polling will stop if the function does not return true.</param>
    /// <param name="pollingInterval">The duration to wait between polling attempts.</param>
    /// <returns>A ValueTask that represents the asynchronous operation. The task result is true if the function returns true before the timeout duration, otherwise false.</returns>
    public static async ValueTask<bool> WaitUntilTrueAsync(Func<Task<bool>> function, TimeSpan timeout,
        TimeSpan pollingInterval)
    {
        var startTime = DateTime.UtcNow;

        while (DateTime.UtcNow - startTime < timeout)
        {
            if (await function())
            {
                return true;
            }

            await Task.Delay(pollingInterval);
        }

        return false;
    }

    /// <summary>
    /// Polls the specified asynchronous function until it returns true or the timeout duration has been reached.
    /// </summary>
    /// <param name="function">The asynchronous function to poll.</param>
    /// <param name="timeout">The duration in milliseconds after which the polling will stop if the function does not return true.</param>
    /// <param name="pollingInterval">The duration in milliseconds to wait between polling attempts.</param>
    /// <returns>A ValueTask that represents the asynchronous operation. The task result is true if the function returns true before the timeout duration, otherwise false.</returns>
    public static async ValueTask<bool> WaitUntilTrueAsync(Func<Task<bool>> function, int timeout,
        int pollingInterval)
    {
        return await WaitUntilTrueAsync(function, TimeSpan.FromMilliseconds(timeout),
            TimeSpan.FromMilliseconds(pollingInterval));
    }

    /// <summary>
    /// Polls the specified synchronous function until it returns true or the timeout duration has been reached.
    /// </summary>
    /// <param name="function">The synchronous function to poll.</param>
    /// <param name="timeout">The duration in milliseconds after which the polling will stop if the function does not return true.</param>
    /// <param name="pollingInterval">The duration in milliseconds to wait between polling attempts.</param>
    /// <returns>A ValueTask that represents the asynchronous operation. The task result is true if the function returns true before the timeout duration, otherwise false.</returns>
    public static async ValueTask<bool> WaitUntilTrueAsync(Func<bool> function, int timeout,
        int pollingInterval)
    {
        return await WaitUntilTrueAsync(function, TimeSpan.FromMilliseconds(timeout),
            TimeSpan.FromMilliseconds(pollingInterval));
    }
}