using System;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Facades;

namespace IOKode.OpinionatedFramework.Jobs;

/// <summary>
/// Defines a job enqueuer contract that allows enqueuing jobs with optional delay, in either the default or a specified queue.
/// </summary>
[AddToFacade("Job")]
public interface IJobEnqueuer
{
    /// <summary>
    /// Enqueues a job in the default queue.
    /// </summary>
    /// <typeparam name="TJob">The type of the job to be enqueued, which must implement <see cref="Job"/>.</typeparam>
    /// <param name="creator">Creator to initialize the job instance.</param>
    /// <param name="cancellationToken">A token to cancel the enqueuing operation (not the job execution).</param>
    /// <returns>A task representing the asynchronous operation of enqueuing the job.</returns>
    public Task EnqueueAsync<TJob>(JobCreator<TJob> creator, CancellationToken cancellationToken = default) where TJob : Job
    {
        return EnqueueAsync(Queue.Default, creator, cancellationToken);
    }

    /// <summary>
    /// Enqueues a job in the default queue with a delay.
    /// </summary>
    /// <typeparam name="TJob">The type of the job to be enqueued, which must implement <see cref="Job"/>.</typeparam>
    /// <param name="delay">The delay duration after which the job execution will begin.</param>
    /// <param name="creator">Creator to initialize the job instance.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation, if needed.</param>
    /// <returns>A task representing the asynchronous operation of enqueuing the job.</returns>
    public Task EnqueueWithDelayAsync<TJob>(TimeSpan delay, JobCreator<TJob> creator, CancellationToken cancellationToken = default) where TJob : Job
    {
        return EnqueueWithDelayAsync(delay, Queue.Default, creator, cancellationToken);
    }

    /// <summary>
    /// Enqueues a job with the specified queue.
    /// </summary>
    /// <typeparam name="TJob">The type of the job to be enqueued, which must implement <see cref="Job"/>.</typeparam>
    /// <param name="queue">The queue where the job will be enqueued. If <see cref="Queue.Default"/> is used, the job will go to the default queue.</param>
    /// <param name="creator">Creator to initialize the job instance.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation, if needed.</param>
    /// <returns>A task representing the asynchronous operation of enqueuing the job.</returns>
    public Task EnqueueAsync<TJob>(Queue queue, JobCreator<TJob> creator, CancellationToken cancellationToken = default) where TJob : Job;

    /// <summary>
    /// Enqueues a job with a specified queue with delay.
    /// </summary>
    /// <typeparam name="TJob">The type of the job to be enqueued, which must implement <see cref="Job"/>.</typeparam>
    /// <param name="delay">The delay duration after which the job should be executed.</param>
    /// <param name="queue">The queue where the job will be enqueued. If <see cref="Queue.Default"/> is used, the job will go to the default queue.</param>
    /// <param name="creator">Creator to initialize the job instance.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation, if needed.</param>
    /// <returns>A task representing the asynchronous operation of enqueuing the job.</returns>
    public Task EnqueueWithDelayAsync<TJob>(TimeSpan delay, Queue queue, JobCreator<TJob> creator, CancellationToken cancellationToken = default) where TJob : Job;
}