using System;
using System.Threading;
using System.Threading.Tasks;
using Cronos;

namespace IOKode.OpinionatedFramework.Jobs.Extensions;

public static class JobArgumentsExtensions
{
    public static async Task ScheduleAsync<TJob>(this JobArguments<TJob> jobArguments, CronExpression interval,
        CancellationToken cancellationToken = default) where TJob : IJob
    {
        var scheduler = Locator.Resolve<IJobScheduler>();
        await scheduler.ScheduleAsync(interval, jobArguments, cancellationToken);
    }

    public static async Task EnqueueAsync<TJob>(this JobArguments<TJob> jobArguments, CancellationToken cancellationToken) where TJob : IJob
    {
        var enqueuer = Locator.Resolve<IJobEnqueuer>();
        await enqueuer.EnqueueAsync(jobArguments, cancellationToken);
    }

    public static async Task EnqueueAsync<TJob>(this JobArguments<TJob> jobArguments, Queue queue, CancellationToken cancellationToken) where TJob : IJob
    {
        var enqueuer = Locator.Resolve<IJobEnqueuer>();
        await enqueuer.EnqueueAsync(queue, jobArguments, cancellationToken);
    }
    
    public static async Task EnqueueWithDelayAsync<TJob>(this JobArguments<TJob> jobArguments, TimeSpan delay, CancellationToken cancellationToken) where TJob : IJob
    {
        var enqueuer = Locator.Resolve<IJobEnqueuer>();
        await enqueuer.EnqueueWithDelayAsync(delay, jobArguments, cancellationToken);
    }

    public static async Task EnqueueWithDelayAsync<TJob>(this JobArguments<TJob> jobArguments, TimeSpan delay, Queue queue, CancellationToken cancellationToken) where TJob : IJob
    {
        var enqueuer = Locator.Resolve<IJobEnqueuer>();
        await enqueuer.EnqueueWithDelayAsync(delay, queue, jobArguments, cancellationToken);
    }
}