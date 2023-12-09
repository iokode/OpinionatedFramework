using System.Threading;
using System.Threading.Tasks;
using Cronos;
using IOKode.OpinionatedFramework.Facades;

namespace IOKode.OpinionatedFramework.Jobs;

[AddToFacade("Job")]
public interface IJobScheduler
{
    public Task<ScheduledJob> ScheduleAsync(IJob job, CronExpression interval, CancellationToken cancellationToken);
    public Task RescheduleAsync(ScheduledJob scheduledJob, CronExpression interval, CancellationToken cancellationToken);
    public Task UnscheduleAsync(ScheduledJob scheduledJob, CancellationToken cancellationToken);
}