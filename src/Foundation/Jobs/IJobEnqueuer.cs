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
    /// <param name="job">The job to be enqueued.</param>
    /// <param name="cancellationToken">A token to cancel the enqueuing process, but it does NOT cancel the job once it is enqueued.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation of enqueueing.</returns>
    public Task EnqueueAsync(IJob job, CancellationToken cancellationToken = default)
    {
        return EnqueueAsync(Queue.Default, job, cancellationToken);
    }

    /// <summary>
    /// Enqueues a job with a specified delay in the default queue.
    /// </summary>
    /// <param name="job">The job to be enqueued.</param>
    /// <param name="delay">The delay before the job is enqueued.</param>
    /// <param name="cancellationToken">A token to cancel the enqueuing process, but it does NOT cancel the job once it is enqueued.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation of enqueueing.</returns>
    public Task EnqueueWithDelayAsync(IJob job, TimeSpan delay, CancellationToken cancellationToken = default)
    {
        return EnqueueWithDelayAsync(Queue.Default, job, delay, cancellationToken);
    }
    
    /// <summary>
    /// Enqueues a job in a specific queue.
    /// </summary>
    /// <param name="queue">The queue where the job will be enqueued.</param>
    /// <param name="job">The job to be enqueued.</param>
    /// <param name="cancellationToken">A token to cancel the enqueuing process, but it does NOT cancel the job once it is enqueued.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation of enqueueing.</returns>
    public Task EnqueueAsync(Queue queue, IJob job, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Enqueues a job with a specified delay in a specific queue.
    /// </summary>
    /// <param name="queue">The queue where the job will be enqueued.</param>
    /// <param name="job">The job to be enqueued.</param>
    /// <param name="delay">The delay before the job is enqueued.</param>
    /// <param name="cancellationToken">A token to cancel the enqueuing process, but it does NOT cancel the job once it is enqueued.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation of enqueueing.</returns>
    public Task EnqueueWithDelayAsync(Queue queue, IJob job, TimeSpan delay, CancellationToken cancellationToken = default);
}