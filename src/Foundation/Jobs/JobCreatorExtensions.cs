using System;
using System.Threading;
using System.Threading.Tasks;
using Cronos;
using IOKode.OpinionatedFramework.ServiceLocation;

namespace IOKode.OpinionatedFramework.Jobs.Extensions;

public static class JobCreatorExtensions
{
    /// <summary>
    /// Schedules the job to run at the specified interval.
    /// </summary>
    /// <returns>
    /// The ID of the scheduled job, which <see cref="IJobScheduler.RescheduleAsync"/> and
    /// <see cref="IJobScheduler.UnscheduleAsync"/> take.
    /// </returns>
    public static async Task<Guid> ScheduleAsync<TJob>(this JobCreator<TJob> jobCreator, CronExpression interval,
        CancellationToken cancellationToken = default) where TJob : Job
    {
        var scheduler = Locator.Resolve<IJobScheduler>();
        return await scheduler.ScheduleAsync(interval, jobCreator, cancellationToken);
    }

    public static async Task EnqueueAsync<TJob>(this JobCreator<TJob> jobCreator, CancellationToken cancellationToken) where TJob : Job
    {
        var enqueuer = Locator.Resolve<IJobEnqueuer>();
        await enqueuer.EnqueueAsync(jobCreator, cancellationToken);
    }

    public static async Task EnqueueAsync<TJob>(this JobCreator<TJob> jobCreator, Queue queue, CancellationToken cancellationToken) where TJob : Job
    {
        var enqueuer = Locator.Resolve<IJobEnqueuer>();
        await enqueuer.EnqueueAsync(queue, jobCreator, cancellationToken);
    }
    
    public static async Task EnqueueWithDelayAsync<TJob>(this JobCreator<TJob> jobCreator, TimeSpan delay, CancellationToken cancellationToken) where TJob : Job
    {
        var enqueuer = Locator.Resolve<IJobEnqueuer>();
        await enqueuer.EnqueueWithDelayAsync(delay, jobCreator, cancellationToken);
    }

    public static async Task EnqueueWithDelayAsync<TJob>(this JobCreator<TJob> jobCreator, TimeSpan delay, Queue queue, CancellationToken cancellationToken) where TJob : Job
    {
        var enqueuer = Locator.Resolve<IJobEnqueuer>();
        await enqueuer.EnqueueWithDelayAsync(delay, queue, jobCreator, cancellationToken);
    }
}
