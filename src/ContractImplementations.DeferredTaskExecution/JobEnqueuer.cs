using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Jobs;

namespace IOKode.OpinionatedFramework.ContractImplementations.DeferredTaskExecution;

public partial class JobEnqueuer : IJobEnqueuer, IDisposable
{
    private static int _eventCounter = 0;
    private readonly ConcurrentDictionary<Queue, SemaphoreSlim> _queueLocks = new();
    private readonly ConcurrentDictionary<Queue, SortedSet<(IJob job, DateTime? executeAt)>> _jobQueues = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private const int PollingIntervalMilliseconds = 100;

    public JobEnqueuer()
    {
        Task.Run(() => ProcessQueuesAsync(_cancellationTokenSource.Token));
    }

    public Task EnqueueAsync(Queue queue, IJob job, CancellationToken cancellationToken)
    {
        var jobQueue = _jobQueues.GetOrAdd(queue, _ => new SortedSet<(IJob, DateTime?)>(new JobComparer()));
        jobQueue.Add((job, null));

        return Task.CompletedTask;
    }

    public Task EnqueueWithDelayAsync(Queue queue, IJob job, TimeSpan delay, CancellationToken cancellationToken)
    {
        var jobQueue = _jobQueues.GetOrAdd(queue, _ => new SortedSet<(IJob, DateTime?)>(new JobComparer()));
        var executeAt = DateTime.UtcNow.Add(delay);
        jobQueue.Add((job, executeAt));

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
    }

    private class JobComparer : IComparer<(IJob job, DateTime? executeAt)>
    {
        public int Compare((IJob job, DateTime? executeAt) x, (IJob job, DateTime? executeAt) y)
        {
            if (x.executeAt == y.executeAt) return 0;
            if (x.executeAt == null) return -1;
            if (y.executeAt == null) return 1;
            return x.executeAt.Value.CompareTo(y.executeAt.Value);
        }
    }
}
