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

    /// <summary>
    /// This method is not intended to be called directly in application code.
    /// Exists to allow Hangfire to serialize and deserialize the job object 
    /// that it will receive as an argument. This ensures that the job 
    /// can be processed correctly during execution.
    /// </summary>
    /// <remarks>
    /// This method must be public because it is used by Hangfire during the deserialization 
    /// and execution of enqueued tasks. Hangfire requires that methods to be invoked 
    /// are publicly accessible to resolve them when deserializing the previously generated expression.
    /// </remarks>
    public static async Task InvokeJob(IJob job)
    {
        await job.InvokeAsync(default);
    }
}