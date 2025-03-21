using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using IOKode.OpinionatedFramework.Facades;
using IOKode.OpinionatedFramework.Jobs;
using Container = IOKode.OpinionatedFramework.Bootstrapping.Container;
using Job = IOKode.OpinionatedFramework.Jobs.Job;

namespace IOKode.OpinionatedFramework.ContractImplementations.Hangfire.Jobs;

public class HangfireJobEnqueuer : IJobEnqueuer
{
    public Task EnqueueAsync<TJob>(Queue queue, JobCreator<TJob> creator, CancellationToken cancellationToken = default)
        where TJob : Job
    {
        // None cancellation token will be replaced internally.
        // https://docs.hangfire.io/en/latest/background-methods/using-cancellation-tokens.html
        BackgroundJob.Enqueue(queue.Name, () => InvokeJobAsync(creator, CancellationToken.None));
        return Task.CompletedTask;
    }

    public Task EnqueueWithDelayAsync<TJob>(TimeSpan delay, Queue queue, JobCreator<TJob> creator,
        CancellationToken cancellationToken = default) where TJob : Job
    {
        // None cancellation token will be replaced internally.
        // https://docs.hangfire.io/en/latest/background-methods/using-cancellation-tokens.html
        BackgroundJob.Schedule(queue.Name, () => InvokeJobAsync(creator, CancellationToken.None), delay);
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
    public static async Task InvokeJobAsync<TJob>(JobCreator<TJob> creator, CancellationToken cancellationToken) where TJob : Job
    {
        try
        {
            Container.Advanced.CreateScope();

            var context = new HangfireJobExecutionContext
            {
                Name = creator.GetJobName(),
                CancellationToken = cancellationToken,
                JobType = typeof(TJob),
                TraceID = Guid.NewGuid(),
            };

            var job = creator.CreateJob();
            await job.ExecuteAsync(context);
        }
        finally
        {
            Container.Advanced.DisposeScope();
        }
    }
}