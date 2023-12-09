using System;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Facades;

namespace IOKode.OpinionatedFramework.Jobs;

[AddToFacade("Job")]
public interface IJobEnqueuer
{
    /// <summary>
    /// Enqueue a job in the default queue.
    /// </summary>
    /// <param name="job"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task EnqueueAsync(IJob job, CancellationToken cancellationToken)
    {
        return EnqueueAsync(Queue.Default, job, cancellationToken);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="job"></param>
    /// <param name="delay"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task EnqueueWithDelayAsync(IJob job, TimeSpan delay, CancellationToken cancellationToken)
    {
        return EnqueueWithDelayAsync(Queue.Default, job, delay, cancellationToken);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="queue"></param>
    /// <param name="job"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task EnqueueAsync(Queue queue, IJob job, CancellationToken cancellationToken);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="queue"></param>
    /// <param name="job"></param>
    /// <param name="delay"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task EnqueueWithDelayAsync(Queue queue, IJob job, TimeSpan delay, CancellationToken cancellationToken);
}