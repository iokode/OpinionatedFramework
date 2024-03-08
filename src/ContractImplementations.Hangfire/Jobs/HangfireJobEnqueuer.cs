using System;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using IOKode.OpinionatedFramework.Jobs;

namespace IOKode.OpinionatedFramework.ContractImplementations.Hangfire.Jobs;

public class HangfireJobEnqueuer : IJobEnqueuer
{
    public Task EnqueueAsync(Queue queue, IJob job, CancellationToken cancellationToken)
    {
        BackgroundJob.Enqueue(queue.Name, () => InvokeJob(job));
        return Task.CompletedTask;
    }

    public Task EnqueueWithDelayAsync(Queue queue, IJob job, TimeSpan delay, CancellationToken cancellationToken)
    {
        BackgroundJob.Schedule(queue.Name, () => InvokeJob(job), delay);
        return Task.CompletedTask;
    }

    public async Task InvokeJob(IJob job)
    {
        await job.InvokeAsync(default);
    }
}