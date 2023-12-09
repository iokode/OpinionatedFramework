using System;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Jobs;

namespace IOKode.OpinionatedFramework.ContractImplementations.SynchronousJobs;

public class SynchronousJobEnqueuer : IJobEnqueuer
{
    public async Task EnqueueAsync(Queue queue, IJob job, CancellationToken cancellationToken)
    {
        await job.InvokeAsync(cancellationToken);
    }

    public Task EnqueueWithDelayAsync(Queue queue, IJob job, TimeSpan delay, CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            await Task.Delay(delay, cancellationToken);
            await job.InvokeAsync(cancellationToken);
        }, cancellationToken);

        return Task.CompletedTask;
    }
}