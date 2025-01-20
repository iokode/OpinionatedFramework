using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cronos;
using IOKode.OpinionatedFramework.Configuration;
using IOKode.OpinionatedFramework.Ensuring;
using IOKode.OpinionatedFramework.Facades;
using IOKode.OpinionatedFramework.Jobs;
using IOKode.OpinionatedFramework.Logging;

namespace IOKode.OpinionatedFramework.ContractImplementations.TaskRunJobs;

public class TaskRunJobScheduler(IConfigurationProvider configuration, ILogging logging) : IJobScheduler
{
    private List<object> registeredJobs = new();

    public Task<ScheduledJob<TJob>> ScheduleAsync<TJob>(CronExpression interval, JobArguments<TJob>? jobArguments, CancellationToken cancellationToken = default) where TJob : IJob
    {
        var scheduledJob = new TaskRunMutableScheduledJob<TJob>(interval, jobArguments);
        this.registeredJobs.Add(scheduledJob);
        var job = Job.Create(jobArguments);

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

        return Task.FromResult((ScheduledJob<TJob>) scheduledJob);
    }
    
    public Task RescheduleAsync<TJob>(ScheduledJob<TJob> scheduledJob, CronExpression interval, CancellationToken cancellationToken = default) where TJob : IJob
    {
        Ensure.Type.IsAssignableTo(scheduledJob.GetType(), typeof(MutableScheduledJob<TJob>))
            .ElseThrowsIllegalArgument($"Type must be assignable to {nameof(MutableScheduledJob<TJob>)} type.", nameof(scheduledJob));

        var mutableScheduledJob = this.registeredJobs.Cast<TaskRunMutableScheduledJob<TJob>>().FirstOrDefault(job => job.Identifier == scheduledJob.Identifier);
        Ensure.Object.NotNull(mutableScheduledJob)
            .ElseThrowsIllegalArgument($"The {nameof(ScheduledJob<TJob>.Identifier)} value was not found on the schedule jobs.", nameof(scheduledJob));

        mutableScheduledJob!.ChangeInterval(interval);
        ((MutableScheduledJob<TJob>) scheduledJob).ChangeInterval(interval);

        return Task.CompletedTask;
    }

    public Task UnscheduleAsync<TJob>(ScheduledJob<TJob> scheduledJob, CancellationToken cancellationToken = default) where TJob : IJob
    {
        var identifier = scheduledJob.Identifier;

        var mutableScheduledJob = this.registeredJobs.Cast<TaskRunMutableScheduledJob<TJob>>().FirstOrDefault(job => job.Identifier == identifier);
        Ensure.Object.NotNull(mutableScheduledJob)
            .ElseThrowsIllegalArgument($"The {nameof(ScheduledJob<TJob>.Identifier)} value was not found on the schedule jobs.", nameof(scheduledJob));

        mutableScheduledJob!.CancelLoop();
        this.registeredJobs.Remove(mutableScheduledJob);

        return Task.CompletedTask;
    }
}