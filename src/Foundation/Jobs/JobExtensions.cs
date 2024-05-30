using System;
using System.Threading;
using System.Threading.Tasks;
using Cronos;
using IOKode.OpinionatedFramework.Facades;

namespace IOKode.OpinionatedFramework.Jobs;

public static class JobExtensions
{
    public static async Task ScheduleAsync(this IJob job, CronExpression interval,
        CancellationToken cancellationToken = default)
    {
        await Job.ScheduleAsync(job, interval, cancellationToken);
    }

    public static async Task EnqueueAsync(this IJob job, string queue, CancellationToken cancellationToken)
    {
        await Job.EnqueueAsync(queue, job, cancellationToken);
    }

    public static async Task EnqueueWithDelayAsync(this IJob job, string queue, TimeSpan delay, CancellationToken cancellationToken)
    {
        await Job.EnqueueWithDelayAsync(queue, job, delay, cancellationToken);
    }
}