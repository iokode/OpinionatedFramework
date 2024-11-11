using System;
using System.Threading;
using System.Threading.Tasks;
using Cronos;

namespace IOKode.OpinionatedFramework.Jobs.Extensions;

public static class JobExtensions
{
    public static async Task ScheduleAsync(this IJob job, CronExpression interval,
        CancellationToken cancellationToken = default)
    {
        var scheduler = Locator.Resolve<IJobScheduler>();
        await scheduler.ScheduleAsync(job, interval, cancellationToken);
    }

    public static async Task EnqueueAsync(this IJob job, Queue queue, CancellationToken cancellationToken)
    {
        var enqueuer = Locator.Resolve<IJobEnqueuer>();
        await enqueuer.EnqueueAsync(queue, job, cancellationToken);
    }

    public static async Task EnqueueWithDelayAsync(this IJob job, Queue queue, TimeSpan delay, CancellationToken cancellationToken)
    {
        var enqueuer = Locator.Resolve<IJobEnqueuer>();
        await enqueuer.EnqueueWithDelayAsync(queue, job, delay, cancellationToken);
    }
}