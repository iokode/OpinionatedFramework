using System;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using IOKode.OpinionatedFramework.Facades;
using IOKode.OpinionatedFramework.Jobs;

namespace IOKode.OpinionatedFramework.ContractImplementations.Hangfire.Jobs;

public class HangfireJobEnqueuer : IJobEnqueuer
{
    public Task EnqueueAsync<TJob>(Queue queue, JobArguments<TJob>? jobArguments, CancellationToken cancellationToken = default) where TJob : IJob
    {
        BackgroundJob.Enqueue(queue.Name, () => InvokeJobAsync(jobArguments));
        return Task.CompletedTask;
    }

    public Task EnqueueWithDelayAsync<TJob>(TimeSpan delay, Queue queue, JobArguments<TJob>? jobArguments, CancellationToken cancellationToken = default) where TJob : IJob
    {
        BackgroundJob.Schedule(queue.Name, () => InvokeJobAsync(jobArguments), delay);
        return Task.CompletedTask;
    }

    /// <summary>
    /// This method is not intended to be called directly in application code.
    /// Exists to allow Hangfire to serialize and deserialize the job arguments 
    /// received on method. This ensures that the job 
    /// can be processed correctly during execution.
    /// </summary>
    /// <remarks>
    /// This method must be public because it is used by Hangfire during the deserialization 
    /// and execution of enqueued tasks. Hangfire requires that methods to be invoked 
    /// are publicly accessible to resolve them when deserializing the previously generated expression.
    /// </remarks>
    public static async Task InvokeJobAsync<TJob>(JobArguments<TJob>? jobArguments) where TJob : IJob
    {
        var job = Job.Create(jobArguments);
        await job.InvokeAsync(default);
    }
}