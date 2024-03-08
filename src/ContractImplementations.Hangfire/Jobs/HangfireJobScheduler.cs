using System.Threading;
using System.Threading.Tasks;
using Cronos;
using Hangfire;
using IOKode.OpinionatedFramework.Ensuring;
using IOKode.OpinionatedFramework.Jobs;

namespace IOKode.OpinionatedFramework.ContractImplementations.Hangfire.Jobs;

public class HangfireJobScheduler : IJobScheduler
{
    public Task<ScheduledJob> ScheduleAsync(IJob job, CronExpression interval, CancellationToken cancellationToken)
    {
        var scheduledJob = new MutableScheduledJob(interval, job);
        RecurringJob.AddOrUpdate(scheduledJob.Identifier.ToString(), () => RunAsync(job), interval.ToString);
        return Task.FromResult<ScheduledJob>(scheduledJob);
    }

    public Task RescheduleAsync(ScheduledJob scheduledJob, CronExpression interval, CancellationToken cancellationToken)
    {
        Ensure.Type.IsAssignableTo(scheduledJob.GetType(), typeof(MutableScheduledJob))
            .ElseThrowsIllegalArgument($"Type must be assignable to {nameof(MutableScheduledJob)} type.", nameof(scheduledJob));

        RecurringJob.RemoveIfExists(scheduledJob.Identifier.ToString());
        RecurringJob.AddOrUpdate(scheduledJob.Identifier.ToString(), () => RunAsync(scheduledJob.Job), interval.ToString);
        ((MutableScheduledJob) scheduledJob).ChangeInterval(interval);

        return Task.CompletedTask;
    }

    public Task UnscheduleAsync(ScheduledJob scheduledJob, CancellationToken cancellationToken)
    {
        RecurringJob.RemoveIfExists(scheduledJob.Identifier.ToString());
        return Task.CompletedTask;
    }

    public async Task RunAsync(IJob job)
    {
        await job.InvokeAsync(default);
    }
}