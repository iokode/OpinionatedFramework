using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Facades;
using IOKode.OpinionatedFramework.Jobs;
using IOKode.OpinionatedFramework.Utilities;
using Microsoft.Extensions.Logging;

namespace IOKode.OpinionatedFramework.ContractImplementations.DeferredTaskExecution;

public partial class JobEnqueuer
{
    private async Task ProcessQueuesAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            foreach (var (queue, jobQueue) in _jobQueues)
            {
                var jobReady = await PollingUtility.WaitUntilTrueAsync(() =>
                {
                    return jobQueue.Count > 0 && 
                           (!jobQueue.Min.executeAt.HasValue || jobQueue.Min.executeAt <= DateTime.UtcNow);
                }, PollingIntervalMilliseconds, cancellationToken);

                if (jobReady)
                {
                    await InvokeJobAsync(queue, jobQueue, cancellationToken);
                }
            }

            await Task.Delay(PollingIntervalMilliseconds, cancellationToken);
        }
    }

    private async Task InvokeJobAsync(Queue queue, SortedSet<(IJob job, DateTime? executeAt)> jobQueue, CancellationToken cancellationToken)
    {
        var semaphore = _queueLocks.GetOrAdd(queue, _ => new SemaphoreSlim(1, 1));

        if (await semaphore.WaitAsync(0, cancellationToken))
        {
            var jobInfo = jobQueue.Min;
            jobQueue.Remove(jobInfo);

            _ = Task.Run(async () =>
            {
                var eventId = new EventId(Interlocked.Increment(ref _eventCounter), "BackgroundJobExecution");
                try
                {
                    await jobInfo.job.InvokeAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    Log.Error(eventId, ex, "An exception has been thrown while running the background job.");
                }
                finally
                {
                    semaphore.Release();
                }
            }, cancellationToken);
        }
    }
}