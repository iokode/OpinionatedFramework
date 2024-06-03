using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Jobs;

namespace IOKode.OpinionatedFramework.Tests.DeferredTaskExecution;

public class JobLogEntry
{
    public required string Queue { get; init; }
    public required string JobId { get; init; }
    public required DateTime StartTime { get; init; }
    public required int ThreadId { get; init; }
    public DateTime EndTime { get; set; }
}

public class ShortRunningTestJob : IJob
{
    private readonly string _jobId;
    private readonly ConcurrentQueue<JobLogEntry> _logQueue;
    private readonly string _queueName;

    public ShortRunningTestJob(string jobId, ConcurrentQueue<JobLogEntry> logQueue, string queueName)
    {
        _jobId = jobId;
        _logQueue = logQueue;
        _queueName = queueName;
    }

    public async Task InvokeAsync(CancellationToken cancellationToken)
    {
        var log = new JobLogEntry
        {
            Queue = _queueName,
            JobId = _jobId,
            StartTime = DateTime.UtcNow,
            ThreadId = Environment.CurrentManagedThreadId
        };

        _logQueue.Enqueue(log);

        await Task.Delay(50); // Simulate job work

        log.EndTime = DateTime.UtcNow;
    }
}

public class LongRunningTestJob : IJob
{
    private readonly string _jobId;
    private readonly ConcurrentQueue<JobLogEntry> _logQueue;
    private readonly string _queueName;
    private readonly TimeSpan _duration;

    public LongRunningTestJob(string jobId, ConcurrentQueue<JobLogEntry> logQueue, string queueName, TimeSpan duration)
    {
        _jobId = jobId;
        _logQueue = logQueue;
        _queueName = queueName;
        _duration = duration;
    }

    public async Task InvokeAsync(CancellationToken cancellationToken)
    {
        var log = new JobLogEntry
        {
            Queue = _queueName,
            JobId = _jobId,
            StartTime = DateTime.UtcNow,
            ThreadId = Environment.CurrentManagedThreadId
        };
        
        _logQueue.Enqueue(log);

        await Task.Delay(_duration, cancellationToken);

        log.EndTime = DateTime.UtcNow;
    }
}