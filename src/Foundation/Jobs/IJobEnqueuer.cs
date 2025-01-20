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
    /// <typeparam name="TJob">The type of the job to be enqueued, which must implement <see cref="IJob"/>.</typeparam>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation, if needed.</param>
    /// <returns>A task representing the asynchronous operation of enqueuing the job.</returns>
    public Task EnqueueAsync<TJob>(CancellationToken cancellationToken = default) where TJob : IJob
    {
        return EnqueueAsync<TJob>(Queue.Default, cancellationToken);
    }

    /// <summary>
    /// Enqueues a job in the default queue and allows providing arguments for the job.
    /// </summary>
    /// <typeparam name="TJob">The type of the job to be enqueued, which must implement <see cref="IJob"/>.</typeparam>
    /// <param name="jobArguments">Arguments to initialize the job instance. If null, a default instance of the job will be used.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation, if needed.</param>
    /// <returns>A task representing the asynchronous operation of enqueuing the job.</returns>
    public Task EnqueueAsync<TJob>(JobArguments<TJob> jobArguments, CancellationToken cancellationToken = default) where TJob : IJob
    {
        return EnqueueAsync(Queue.Default, jobArguments, cancellationToken);
    }

    /// <summary>
    /// Enqueues a job in the specified queue.
    /// </summary>
    /// <typeparam name="TJob">The type of the job to be enqueued, which must implement <see cref="IJob"/>.</typeparam>
    /// <param name="queue">The queue where the job will be enqueued. If <see cref="Queue.Default"/> is used, the job will be enqueued in the default queue.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation, if needed.</param>
    /// <returns>A task representing the asynchronous operation of enqueuing the job.</returns>
    public Task EnqueueAsync<TJob>(Queue queue, CancellationToken cancellationToken = default) where TJob : IJob
    {
        return EnqueueAsync<TJob>(queue, null, cancellationToken);
    }

    /// <summary>
    /// Enqueues a job in the default queue, with a delay before execution.
    /// </summary>
    /// <typeparam name="TJob">The type of the job to be enqueued, which must implement <see cref="IJob"/>.</typeparam>
    /// <param name="delay">The delay duration after which the job execution will begin.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation, if needed.</param>
    /// <returns>A task representing the asynchronous operation of enqueuing the job.</returns>
    public Task EnqueueWithDelayAsync<TJob>(TimeSpan delay, CancellationToken cancellationToken = default) where TJob : IJob
    {
        return EnqueueWithDelayAsync<TJob>(delay, Queue.Default, cancellationToken);
    }

    /// <summary>
    /// Enqueues a job in the default queue with a delay, providing arguments for the job.
    /// </summary>
    /// <typeparam name="TJob">The type of the job to be enqueued, which must implement <see cref="IJob"/>.</typeparam>
    /// <param name="delay">The delay duration after which the job execution will begin.</param>
    /// <param name="jobArguments">Arguments to initialize the job instance. If null, a default instance of the job will be used.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation, if needed.</param>
    /// <returns>A task representing the asynchronous operation of enqueuing the job.</returns>
    public Task EnqueueWithDelayAsync<TJob>(TimeSpan delay, JobArguments<TJob> jobArguments, CancellationToken cancellationToken = default) where TJob : IJob
    {
        return EnqueueWithDelayAsync(delay, Queue.Default, jobArguments, cancellationToken);
    }

    /// <summary>
    /// Enqueues a job in the specified queue with a delay before execution.
    /// </summary>
    /// <typeparam name="TJob">The type of the job to be enqueued, which must implement <see cref="IJob"/>.</typeparam>
    /// <param name="queue">The queue where the job will be enqueued. If <see cref="Queue.Default"/> is used, the job will be enqueued in the default queue.</param>
    /// <param name="delay">The delay duration after which the job execution will begin.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation, if needed.</param>
    /// <returns>A task representing the asynchronous operation of enqueuing the job.</returns>
    public Task EnqueueWithDelayAsync<TJob>(TimeSpan delay, Queue queue, CancellationToken cancellationToken = default) where TJob : IJob
    {
        return EnqueueWithDelayAsync<TJob>(delay, queue, null, cancellationToken);
    }

    /// <summary>
    /// Enqueues a job with the specified queue, providing optional arguments for the job.
    /// </summary>
    /// <typeparam name="TJob">The type of the job to be enqueued, which must implement <see cref="IJob"/>.</typeparam>
    /// <param name="queue">The queue where the job will be enqueued. If <see cref="Queue.Default"/> is used, the job will go to the default queue.</param>
    /// <param name="jobArguments">Optional arguments for creating the job instance. If null, a default instance of the job will be used.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation, if needed.</param>
    /// <returns>A task representing the asynchronous operation of enqueuing the job.</returns>
    public Task EnqueueAsync<TJob>(Queue queue, JobArguments<TJob>? jobArguments, CancellationToken cancellationToken = default) where TJob : IJob;

    /// <summary>
    /// Enqueues a job with a specified delay and queue, providing optional arguments for the job.
    /// </summary>
    /// <typeparam name="TJob">The type of the job to be enqueued, which must implement <see cref="IJob"/>.</typeparam>
    /// <param name="delay">The delay duration after which the job should be executed.</param>
    /// <param name="queue">The queue where the job will be enqueued. If <see cref="Queue.Default"/> is used, the job will go to the default queue.</param>
    /// <param name="jobArguments">Optional arguments for creating the job instance. If null, a default instance of the job will be used.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation, if needed.</param>
    /// <returns>A task representing the asynchronous operation of enqueuing the job.</returns>
    public Task EnqueueWithDelayAsync<TJob>(TimeSpan delay, Queue queue, JobArguments<TJob>? jobArguments, CancellationToken cancellationToken = default) where TJob : IJob;
}