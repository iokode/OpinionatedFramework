using System;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Facades;

namespace IOKode.OpinionatedFramework.Jobs;

[AddToFacade("Job")]
public interface IJobEnqueuer
{
    public Task EnqueueAsync(Queue queue, IJob job, CancellationToken cancellationToken);
    public Task EnqueueWithDelayAsync(Queue queue, IJob job, TimeSpan delay, CancellationToken cancellationToken);
}