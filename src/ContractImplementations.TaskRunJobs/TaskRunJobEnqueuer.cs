using System;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Jobs;

namespace IOKode.OpinionatedFramework.ContractImplementations.TaskRunJobs;

public class TaskRunJobEnqueuer : IJobEnqueuer
{
    public async Task EnqueueAsync(Queue queue, IJob job, CancellationToken cancellationToken)
    {
        _ = RetryHelper.RetryOnException(job, 10);
    }

    public Task EnqueueWithDelayAsync(Queue queue, IJob job, TimeSpan delay, CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            await Task.Delay(delay, cancellationToken);

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            await RetryHelper.RetryOnException(job, 10);
        }, cancellationToken);

        return Task.CompletedTask;
    }
}