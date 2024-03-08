using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Jobs;

namespace IOKode.OpinionatedFramework.ContractImplementations.TaskRunJobs;

internal static class RetryHelper
{
    public static async Task RetryOnException(IJob job, int maxAttempts)
    {
        bool shouldRetry = false;
        int attempt = 0;
        var exceptions = new List<Exception>();

        do
        {
            try
            {
                attempt++;
                await job.InvokeAsync(default);
                shouldRetry = false;
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
                
                if (attempt < maxAttempts)
                {
                    shouldRetry = true;
                }
                else
                {
                    throw new AggregateException(exceptions);
                }
            }
        } while (shouldRetry);
    }
}