using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cronos;
using IOKode.OpinionatedFramework.Ensuring;
using IOKode.OpinionatedFramework.Jobs;

namespace IOKode.OpinionatedFramework.ContractImplementations.SynchronousJobs;

public class SynchronousJobScheduler : IJobScheduler
{
    private class SynchronousMutableScheduledJob : MutableScheduledJob
    {
        public DateTime LastInvocation { get; set; }
        public bool IsFinalized { get; private set; }

        public SynchronousMutableScheduledJob(CronExpression interval, IJob job) : base(interval, job)
        {
            LastInvocation = DateTime.UtcNow;
        }

        public void CancelLoop()
        {
            IsFinalized = true;
        }
    }

    private List<SynchronousMutableScheduledJob> registeredJobs = new List<SynchronousMutableScheduledJob>();

    public Task<ScheduledJob> ScheduleAsync(IJob job, CronExpression interval, CancellationToken cancellationToken)
    {
        var scheduledJob = new SynchronousMutableScheduledJob(interval, job);
        this.registeredJobs.Add(scheduledJob);
        Task.Run(async () =>
        {
            while (!scheduledJob.IsFinalized)
            {
                var now = DateTime.UtcNow;
                var nextOccurrence = scheduledJob.Interval.GetNextOccurrence(scheduledJob.LastInvocation);
                if (now >= nextOccurrence)
                {
                    // This call is not awaited because we want a "fire it and forget" behaviour.
                    // The correct behaviour is invoke it, but not await to be finalized.
                    scheduledJob.Job.InvokeAsync(default);
                    scheduledJob.LastInvocation = now;
                }

                var delay = scheduledJob.Interval.GetNextOccurrence(scheduledJob.LastInvocation) - now;
                if (delay is null)
                {
                    scheduledJob.CancelLoop();
                    this.registeredJobs.Remove(scheduledJob);
                    break;
                }

                await Task.Delay(delay.Value, default);
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