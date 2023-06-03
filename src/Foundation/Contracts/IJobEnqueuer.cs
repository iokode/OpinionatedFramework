using System;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Jobs;

namespace IOKode.OpinionatedFramework.Contracts;

[AddToFacade("Job")]
public interface IJobEnqueuer
{
    public Task EnqueueAsync(string queue, IJob job, CancellationToken cancellationToken);
    public Task EnqueueWithDelayAsync(string queue, IJob job, TimeSpan delay, CancellationToken cancellationToken);
}