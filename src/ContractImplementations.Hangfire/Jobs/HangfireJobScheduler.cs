using System.Threading;
using System.Threading.Tasks;
using Cronos;
using Hangfire;
using IOKode.OpinionatedFramework.Jobs;

namespace IOKode.OpinionatedFramework.ContractImplementations.Hangfire.Jobs;

public class HangfireJobScheduler : IJobScheduler
{
    // public Task ScheduleAsync(IJob job, CronExpression interval, CancellationToken cancellationToken)
    // {
    //     RecurringJob.AddOrUpdate("", () => job.InvokeAsync(CancellationToken.None), interval.ToString); // todo id
    //     return Task.CompletedTask;
    // }

    public Task RescheduleAsync(ScheduledJob scheduledJob, CronExpression interval, CancellationToken cancellationToken)
    {
        throw new System.NotImplementedException();
    }

    public Task UnscheduleAsync(ScheduledJob scheduledJob, CancellationToken cancellationToken)
    {
        throw new System.NotImplementedException();
    }

    Task<ScheduledJob> IJobScheduler.ScheduleAsync(IJob job, CronExpression interval, CancellationToken cancellationToken)
    {
        throw new System.NotImplementedException();
    }
}