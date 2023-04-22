using System;
using System.Threading.Tasks;

namespace IOKode.OpinionatedFramework.Foundation;

public static class Poll
{
    public static async Task<bool> UntilReturnsTrueAsync(Func<bool> function, TimeSpan timeout, TimeSpan pollingInterval)
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

    public static async Task<bool> UntilReturnsTrueAsync(Func<Task<bool>> function, TimeSpan timeout,
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

    public static async Task<bool> UntilReturnsTrueAsync(Func<Task<bool>> function, int timeout, int pollingInterval)
    {
        return await UntilReturnsTrueAsync(function, TimeSpan.FromMilliseconds(timeout),
            TimeSpan.FromMilliseconds(pollingInterval));
    }

    public static async Task<bool> UntilReturnsTrueAsync(Func<bool> function, int timeout, int pollingInterval)
    {
        return await UntilReturnsTrueAsync(function, TimeSpan.FromMilliseconds(timeout),
            TimeSpan.FromMilliseconds(pollingInterval));
    }
}