using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cronos;
using IOKode.OpinionatedFramework.Configuration;
using IOKode.OpinionatedFramework.Ensuring;
using IOKode.OpinionatedFramework.Jobs;
using IOKode.OpinionatedFramework.Logging;
using Microsoft.Extensions.Hosting;
using NodaTime;
using Job = IOKode.OpinionatedFramework.Jobs.Job;

namespace IOKode.OpinionatedFramework.ContractImplementations.TaskRunJobs;

public class TaskRunJobScheduler(IConfigurationProvider configuration, ILogging logging) : IJobScheduler, IHostedService, IAsyncDisposable
{
    private readonly Lock sync = new();
    private readonly Dictionary<Guid, TaskRunScheduledJob> registeredJobs = new();
    private readonly CancellationTokenSource lifetimeCancellationTokenSource = new();
    private bool acceptsJobs = true;

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (this.sync)
        {
            if (this.lifetimeCancellationTokenSource.IsCancellationRequested)
            {
                throw new InvalidOperationException("The job scheduler cannot be restarted after it has been stopped.");
            }

            this.acceptsJobs = true;
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return this.DisposeAsync().AsTask();
    }

    public async ValueTask DisposeAsync()
    {
        TaskRunScheduledJob[] scheduledJobs;
        lock (this.sync)
        {
            this.acceptsJobs = false;
            scheduledJobs = this.registeredJobs.Values.ToArray();
        }

        foreach (var scheduledJob in scheduledJobs)
        {
            scheduledJob.CancelLoop();
            await scheduledJob.CancelDelayTokenAsync();
        }

        await this.lifetimeCancellationTokenSource.CancelAsync();
        var runningTasks = scheduledJobs
            .Select(scheduledJob => scheduledJob.RunningTask)
            .Where(task => task is not null)
            .Cast<Task>()
            .ToArray();
        if (runningTasks.Length > 0)
        {
            try
            {
                await Task.WhenAll(runningTasks);
            }
            catch (OperationCanceledException) when (this.lifetimeCancellationTokenSource.IsCancellationRequested)
            {
            }
        }

        lock (this.sync)
        {
            this.registeredJobs.Clear();
        }
    }

    public Task<Guid> ScheduleAsync<TJob>(CronExpression interval, JobCreator<TJob> creator,
        CancellationToken cancellationToken = default) where TJob : Job
    {
        cancellationToken.ThrowIfCancellationRequested();
        var scheduledJobId = Guid.NewGuid();
        var scheduledJob = new TaskRunScheduledJob
        {
            Interval = interval
        };
        lock (this.sync)
        {
            if (!this.acceptsJobs)
            {
                throw new InvalidOperationException("The job scheduler has been stopped.");
            }

            this.registeredJobs.Add(scheduledJobId, scheduledJob);
        }

        scheduledJob.RunningTask = this.RunJobAsync(creator, scheduledJob);
        return Task.FromResult(scheduledJobId);
    }

    public async Task RescheduleAsync(Guid scheduledJobId, CronExpression interval,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        TaskRunScheduledJob? scheduledJob;
        lock (this.sync)
        {
            this.registeredJobs.TryGetValue(scheduledJobId, out scheduledJob);
        }

        Ensure.Object.NotNull(scheduledJob)
            .ElseThrowsIllegalArgument($"The id '{scheduledJobId}' was not found on the schedule jobs.", nameof(scheduledJobId));

        scheduledJob!.Interval = interval;
        await scheduledJob.CancelDelayTokenAsync();
    }

    public async Task UnscheduleAsync(Guid scheduledJobId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        TaskRunScheduledJob? scheduledJob;
        lock (this.sync)
        {
            this.registeredJobs.TryGetValue(scheduledJobId, out scheduledJob);
        }

        Ensure.Object.NotNull(scheduledJob)
            .ElseThrowsIllegalArgument($"The id '{scheduledJobId}' was not found on the schedule jobs.", nameof(scheduledJobId));

        scheduledJob!.CancelLoop();
        await scheduledJob.CancelDelayTokenAsync();
        if (scheduledJob.RunningTask is not null)
        {
            await scheduledJob.RunningTask.WaitAsync(cancellationToken);
        }

        lock (this.sync)
        {
            this.registeredJobs.Remove(scheduledJobId);
        }
    }

    private Task RunJobAsync<TJob>(JobCreator<TJob> creator, TaskRunScheduledJob scheduledJob) where TJob : Job
    {
        return Task.Run(async () =>
        {
            while (!scheduledJob.IsFinalized)
            {
                var now = DateTime.UtcNow;
                var nextOccurrence = scheduledJob.NextOccurrence;
                if (nextOccurrence is null)
                {
                    throw new FormatException("The cron expression next occurrence is not found.");
                }

                if (now >= nextOccurrence)
                {
                    try
                    {
                        await RetryHelper.RetryOnExceptionAsync(
                            creator,
                            configuration.GetValue<int>("OpinionatedFramework:JobScheduler:MaxAttempts"),
                            this.lifetimeCancellationTokenSource.Token);
                    }
                    catch (AggregateException exception)
                    {
                        logging.Error(exception, "A scheduled job reached max attempts.");
                    }

                    scheduledJob.LastInvocation = Instant.FromDateTimeUtc(now);
                }

                var delay = scheduledJob.NextOccurrence! - now;
                try
                {
                    await Task.Delay(delay.Value, scheduledJob.DelayCancellationTokenSource.Token);
                }
                catch (TaskCanceledException)
                {
                }
            }
        }, this.lifetimeCancellationTokenSource.Token);
    }
}

internal class TaskRunScheduledJob
{
    public required CronExpression Interval { get; set; }
    public Instant LastInvocation { get; set; } = Instant.FromDateTimeUtc(DateTime.UtcNow);
    public CancellationTokenSource DelayCancellationTokenSource { get; } = new();
    public bool IsFinalized { get; private set; }
    public Task? RunningTask { get; set; }

    public DateTime? NextOccurrence => Interval.GetNextOccurrence(LastInvocation.ToDateTimeUtc());

    public void CancelLoop()
    {
        this.IsFinalized = true;
    }

    public async Task CancelDelayTokenAsync()
    {
        this.LastInvocation = Instant.FromDateTimeUtc(DateTime.UtcNow);
        await this.DelayCancellationTokenSource.CancelAsync();
        this.DelayCancellationTokenSource.TryReset();
    }
}
