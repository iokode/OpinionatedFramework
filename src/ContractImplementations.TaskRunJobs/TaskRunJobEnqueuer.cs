using System;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Configuration;
using IOKode.OpinionatedFramework.Facades;
using IOKode.OpinionatedFramework.Jobs;

namespace IOKode.OpinionatedFramework.ContractImplementations.TaskRunJobs;

public class TaskRunJobEnqueuer(IConfigurationProvider configuration) : IJobEnqueuer
{
    public Task EnqueueAsync<TJob>(Queue queue, JobArguments<TJob>? jobArguments, CancellationToken cancellationToken = default) where TJob : IJob
    {
        var job = Job.Create(jobArguments);
        _ = RetryHelper.RetryOnExceptionAsync(job, configuration.GetValue<int>("TaskRun:JobEnqueuer:MaxAttempts"));
        return Task.CompletedTask;
    }

    public Task EnqueueWithDelayAsync<TJob>(TimeSpan delay, Queue queue, JobArguments<TJob>? jobArguments, CancellationToken cancellationToken = default) where TJob : IJob
    {
        var job = Job.Create(jobArguments);
        _ = Task.Run(async () =>
        {
            await Task.Delay(delay, cancellationToken);

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            await RetryHelper.RetryOnExceptionAsync(job, configuration.GetValue<int>("TaskRun:JobEnqueuer:MaxAttempts"));
        }, cancellationToken);

        return Task.CompletedTask;
    }
}