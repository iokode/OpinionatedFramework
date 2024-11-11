using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cronos;
using IOKode.OpinionatedFramework.Configuration;
using IOKode.OpinionatedFramework.Ensuring;
using IOKode.OpinionatedFramework.Jobs;
using IOKode.OpinionatedFramework.Logging;

namespace IOKode.OpinionatedFramework.ContractImplementations.TaskRunJobs;

public class TaskRunJobScheduler(IConfigurationProvider configuration, ILogging logging) : IJobScheduler
{
    private List<TaskRunMutableScheduledJob> registeredJobs = new();

    public Task<ScheduledJob> ScheduleAsync(IJob job, CronExpression interval, CancellationToken cancellationToken)
    {
        var scheduledJob = new TaskRunMutableScheduledJob(interval, job);
        this.registeredJobs.Add(scheduledJob);
        Task.Run(async () =>
        {
            while (!scheduledJob.IsFinalized)
            {
                var now = DateTime.UtcNow;
                var nextOccurrence = scheduledJob.Interval.GetNextOccurrence(scheduledJob.LastInvocation);

                if (nextOccurrence == null)
                {
                    throw new FormatException("The cron expression next occurrence is not found.");
                }
                
                if (now >= nextOccurrence)
                {
                    try
                    {
                        await RetryHelper.RetryOnExceptionAsync(job, configuration.GetValue<int>("TaskRun:JobScheduler:MaxAttempts"));
                    }
                    catch (AggregateException ex)
                    {
                        logging.Error(ex, "An scheduled job reached max attempts.");
                    }

                    scheduledJob.LastInvocation = now;
                }

                var delay = scheduledJob.Interval.GetNextOccurrence(scheduledJob.LastInvocation)! - now;
                await Task.Delay(delay.Value);
            }
        }, cancellationToken);

        return Task.FromResult((ScheduledJob) scheduledJob);
    }

    public Task RescheduleAsync(ScheduledJob scheduledJob, CronExpression interval, CancellationToken cancellationToken)
    {
        Ensure.Type.IsAssignableTo(scheduledJob.GetType(), typeof(MutableScheduledJob))
            .ElseThrowsIllegalArgument($"Type must be assignable to {nameof(MutableScheduledJob)} type.", nameof(scheduledJob));

        var mutableScheduledJob = this.registeredJobs.Find(j => j.Identifier == scheduledJob.Identifier);
        Ensure.Object.NotNull(mutableScheduledJob)
            .ElseThrowsIllegalArgument($"The {nameof(ScheduledJob.Identifier)} value was not found on the schedule jobs.", nameof(scheduledJob));

        mutableScheduledJob!.ChangeInterval(interval);
        ((MutableScheduledJob) scheduledJob).ChangeInterval(interval);

        return Task.CompletedTask;
    }

    public Task UnscheduleAsync(ScheduledJob scheduledJob, CancellationToken cancellationToken)
    {
        var identifier = scheduledJob.Identifier;

        var mutableScheduledJob = this.registeredJobs.Find(j => j.Identifier == identifier);
        Ensure.Object.NotNull(mutableScheduledJob)
            .ElseThrowsIllegalArgument($"The {nameof(ScheduledJob.Identifier)} value was not found on the schedule jobs.", nameof(scheduledJob));

        mutableScheduledJob!.CancelLoop();
        this.registeredJobs.Remove(mutableScheduledJob);

        return Task.CompletedTask;
    }
}