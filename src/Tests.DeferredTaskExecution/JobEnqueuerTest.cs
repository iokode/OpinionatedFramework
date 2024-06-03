using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.ContractImplementations.DeferredTaskExecution;
using IOKode.OpinionatedFramework.Jobs;
using Xunit;

namespace IOKode.OpinionatedFramework.Tests.DeferredTaskExecution;

public class JobEnqueuerTest
{
    private readonly JobEnqueuer _jobEnqueuer;
    private readonly Queue _queue;

    public JobEnqueuerTest()
    {
        _jobEnqueuer = new JobEnqueuer();
        _queue = Queue.FromName("TestQueue");
    }

    [Fact]
    public async Task EnqueueAsync_ShouldExecuteJobInBackground()
    {
        // Arrange
        var logQueue = new ConcurrentQueue<JobLogEntry>();
        var job = new ShortRunningTestJob("Job1", logQueue, _queue.Name);
        var cancellationToken = default(CancellationToken);

        // Act
        await _jobEnqueuer.EnqueueAsync(_queue, job, cancellationToken);
        await Task.Delay(200); // Allow time for the job to complete

        // Assert
        var logEntries = logQueue.ToArray();
        Assert.Single(logEntries);
        Assert.Equal("Job1", logEntries[0].JobId);
        Assert.NotEqual(Environment.CurrentManagedThreadId, logEntries[0].ThreadId);
        Assert.Equal(_queue.Name, logEntries[0].Queue);
        Assert.True(logEntries[0].StartTime <= logEntries[0].EndTime);
    }

    [Fact]
    public async Task EnqueueWithDelayAsync_ShouldExecuteJobAfterDelayInBackground()
    {
        // Arrange
        var logQueue = new ConcurrentQueue<JobLogEntry>();
        var job = new ShortRunningTestJob("Job1", logQueue, _queue.Name);
        var cancellationToken = default(CancellationToken);
        var delay = TimeSpan.FromSeconds(3);

        // Act
        var enqueueAt = DateTime.UtcNow;
        await _jobEnqueuer.EnqueueWithDelayAsync(_queue, job, delay, cancellationToken);
        await Task.Delay(5000); // Allow time for the job to complete

        // Assert
        var logEntries = logQueue.ToArray();
        Assert.Single(logEntries);
        Assert.True(logEntries[0].StartTime > enqueueAt.Add(delay)); // The job starts AFTER the delay
    }

    [Fact]
    public async Task Jobs_ShouldBeExecutedInOrderInBackground()
    {
        // Arrange
        var logQueue = new ConcurrentQueue<JobLogEntry>();
        var job1 = new ShortRunningTestJob("Job1", logQueue, _queue.Name);
        var job2 = new ShortRunningTestJob("Job2", logQueue, _queue.Name);

        var cancellationToken = default(CancellationToken);
        var delay1 = TimeSpan.FromMilliseconds(100);
        var delay2 = TimeSpan.FromMilliseconds(200);

        // Act
        await _jobEnqueuer.EnqueueWithDelayAsync(_queue, job1, delay1, cancellationToken);
        await _jobEnqueuer.EnqueueWithDelayAsync(_queue, job2, delay2, cancellationToken);
        await Task.Delay(500); // Allow time for the jobs to complete

        // Assert
        var logEntries = logQueue.ToArray();
        Assert.Equal(2, logEntries.Length);
        Assert.Equal("Job1", logEntries[0].JobId);
        Assert.Equal("Job2", logEntries[1].JobId);
        Assert.True(logEntries[0].EndTime <= logEntries[1].StartTime);
    }

    [Fact]
    public async Task OnlyOneJob_ShouldExecuteAtATime_PerQueueInBackground()
    {
        // Arrange
        var logQueue = new ConcurrentQueue<JobLogEntry>();
        var longRunningJob = new LongRunningTestJob("LongJob", logQueue, _queue.Name, TimeSpan.FromMilliseconds(200));
        var job1 = new ShortRunningTestJob("Job1", logQueue, _queue.Name);
        var job2 = new ShortRunningTestJob("Job2", logQueue, _queue.Name);

        var cancellationToken = default(CancellationToken);

        // Act
        await _jobEnqueuer.EnqueueAsync(_queue, longRunningJob, cancellationToken);
        await _jobEnqueuer.EnqueueAsync(_queue, job1, cancellationToken);
        await _jobEnqueuer.EnqueueAsync(_queue, job2, cancellationToken);
        await Task.Delay(1000); // Allow time for the jobs to complete

        // Assert
        var logEntries = logQueue.ToArray();
        Assert.Equal(3, logEntries.Length);
        Assert.Equal("LongJob", logEntries[0].JobId);
        Assert.Equal("Job1", logEntries[1].JobId);
        Assert.Equal("Job2", logEntries[2].JobId);
        Assert.True(logEntries[0].EndTime <= logEntries[1].StartTime);
        Assert.True(logEntries[1].EndTime <= logEntries[2].StartTime);
    }

    [Fact]
    public async Task MultipleQueues_ShouldExecuteJobsConcurrently()
    {
        // Arrange
        var logQueue = new ConcurrentQueue<JobLogEntry>();
        var job1 = new ShortRunningTestJob("Job1", logQueue, "Queue1");
        var job2 = new LongRunningTestJob("Job2", logQueue, "Queue2", TimeSpan.FromMilliseconds(200));
        var job3 = new ShortRunningTestJob("Job3", logQueue, "Queue1");

        var queue1 = Queue.FromName("Queue1");
        var queue2 = Queue.FromName("Queue2");

        var cancellationToken = default(CancellationToken);

        // Act
        await _jobEnqueuer.EnqueueAsync(queue1, job1, cancellationToken);
        await _jobEnqueuer.EnqueueAsync(queue2, job2, cancellationToken);
        await _jobEnqueuer.EnqueueAsync(queue1, job3, cancellationToken);
        await Task.Delay(500); // Allow time for the jobs to complete

        // Assert
        var logEntries = logQueue.ToArray();
        Assert.Equal(3, logEntries.Length);
        Assert.Equal("Job1", logEntries[0].JobId);
        Assert.Equal("Job2", logEntries[1].JobId);
        Assert.Equal("Job3", logEntries[2].JobId);
        Assert.True(logEntries[0].StartTime <= logEntries[2].StartTime); // Ensure Job1 starts before Job3
        Assert.True(logEntries[0].EndTime <= logEntries[2].StartTime); // Ensure Job1 ends before Job3 starts
    }

    [Fact]
    public async Task MultipleJobsInSingleQueue_ShouldExecuteSequentially()
    {
        // Arrange
        var logQueue = new ConcurrentQueue<JobLogEntry>();
        var jobs = new List<ShortRunningTestJob>();
        var cancellationToken = default(CancellationToken);

        for (int i = 0; i < 20; i++)
        {
            jobs.Add(new ShortRunningTestJob($"Job{i + 1}", logQueue, _queue.Name));
        }

        // Act
        foreach (var job in jobs)
        {
            await _jobEnqueuer.EnqueueAsync(_queue, job, cancellationToken);
        }

        await Task.Delay(3000); // Allow time for all jobs to complete

        // Assert
        var logEntries = logQueue.ToArray();
        Assert.Equal(20, logEntries.Length); // Each job should have a single log entry

        for (int i = 1; i < logEntries.Length; i++)
        {
            Assert.True(logEntries[i - 1].EndTime <= logEntries[i].StartTime,
                $"Job {logEntries[i - 1].JobId} did not finish before job {logEntries[i].JobId} started.");
        }
    }
}