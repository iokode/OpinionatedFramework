using System;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using IOKode.OpinionatedFramework.Jobs;

namespace IOKode.OpinionatedFramework.ContractImplementations.Hangfire.Jobs;

public class HangfireJobEnqueuer : IJobEnqueuer
{
    private readonly IBackgroundJobClient backgroundJobClient;

    public HangfireJobEnqueuer(IBackgroundJobClient backgroundJobClient)
    {
        this.backgroundJobClient = backgroundJobClient;
    }

    public Task EnqueueAsync(Queue queue, IJob job, CancellationToken cancellationToken)
    {
        this.backgroundJobClient.Enqueue(queue.Identifier.ToString(), () => job.InvokeAsync(cancellationToken));
        return Task.CompletedTask;
    }

    public Task EnqueueWithDelayAsync(Queue queue, IJob job, TimeSpan delay, CancellationToken cancellationToken)
    {
        this.backgroundJobClient.Schedule<IJob>(queue.Identifier.ToString(), j => j.InvokeAsync(CancellationToken.None), delay);
        return Task.CompletedTask;
    }
}