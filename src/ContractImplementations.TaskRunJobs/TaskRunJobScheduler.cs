using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cronos;
using IOKode.OpinionatedFramework.Configuration;
using IOKode.OpinionatedFramework.Ensuring;
using IOKode.OpinionatedFramework.Jobs;
using IOKode.OpinionatedFramework.Logging;
using NodaTime;
using Job = IOKode.OpinionatedFramework.Jobs.Job;

namespace IOKode.OpinionatedFramework.ContractImplementations.TaskRunJobs;

public class TaskRunJobScheduler(IConfigurationProvider configuration, ILogging logging) : IJobScheduler
{
    private readonly Dictionary<Guid, TaskRunScheduledJob> registeredJobs = new();

    public Task<Guid> ScheduleAsync<TJob>(CronExpression interval, JobCreator<TJob> creator, CancellationToken cancellationToken = default) where TJob : Job
    {
        var scheduledJobId = Guid.NewGuid();
        var scheduledJob = new TaskRunScheduledJob
        {
            Interval = interval
        };
        this.registeredJobs.Add(scheduledJobId, scheduledJob);
        RunJobAsync(creator, scheduledJob, cancellationToken);

        return Task.FromResult(scheduledJobId);
    }

    public async Task RescheduleAsync(Guid scheduledJobId, CronExpression interval, CancellationToken cancellationToken = default)
    {
        this.registeredJobs.TryGetValue(scheduledJobId, out var scheduledJob);
        Ensure.Object.NotNull(scheduledJob)
            .ElseThrowsIllegalArgument($"The id '{scheduledJobId}' was not found on the schedule jobs.", nameof(scheduledJobId));

        scheduledJob!.Interval = interval;
        await scheduledJob.CancelDelayTokenAsync();
    }

    public async Task UnscheduleAsync(Guid scheduledJobId, CancellationToken cancellationToken = default)
    {
        this.registeredJobs.TryGetValue(scheduledJobId, out var scheduledJob);
        Ensure.Object.NotNull(scheduledJob)
            .ElseThrowsIllegalArgument($"The id '{scheduledJobId}' was not found on the schedule jobs.", nameof(scheduledJobId));

        scheduledJob!.CancelLoop();
        await scheduledJob.CancelDelayTokenAsync();
        this.registeredJobs.Remove(scheduledJobId);
    }

    private Task RunJobAsync<TJob>(JobCreator<TJob> creator, TaskRunScheduledJob scheduledJob, CancellationToken cancellationToken) where TJob : Job
    {
        return Task.Run(async () =>
        {
            while (!scheduledJob.IsFinalized)
            {
                var now = DateTime.UtcNow;
                var nextOccurrence = scheduledJob.NextOccurrence;

                if (nextOccurrence == null)
                {
                    throw new FormatException("The cron expression next occurrence is not found.");
                }

                if (now >= nextOccurrence)
                {
                    try
                    {
                        await RetryHelper.RetryOnExceptionAsync(creator, configuration.GetValue<int>("TaskRun:JobScheduler:MaxAttempts"));
                    }
                    catch (AggregateException ex)
                    {
                        logging.Error(ex, "An scheduled job reached max attempts.");
                    }

                    scheduledJob.LastInvocation = Instant.FromDateTimeUtc(now);
                }

                var delay = scheduledJob.NextOccurrence! - now;
                await Task.Delay(delay.Value, scheduledJob.DelayCancellationTokenSource.Token);
            }
        }, cancellationToken);
    }
}

internal class TaskRunScheduledJob
{
    public required CronExpression Interval { get; set; }
    public Instant LastInvocation { get; set; } = Instant.FromDateTimeUtc(DateTime.UtcNow);
    public bool IsFinalized { get; private set; }
    public CancellationTokenSource DelayCancellationTokenSource { get; private set; } = new();

    public DateTime? NextOccurrence => Interval.GetNextOccurrence(LastInvocation.ToDateTimeUtc())!.Value;

    public void CancelLoop()
    {
        IsFinalized = true;
    }

    public async Task CancelDelayTokenAsync()
    {
        LastInvocation = Instant.FromDateTimeUtc(DateTime.UtcNow);
        await DelayCancellationTokenSource.CancelAsync();
        DelayCancellationTokenSource.Dispose();
        DelayCancellationTokenSource = new CancellationTokenSource();
    }
}