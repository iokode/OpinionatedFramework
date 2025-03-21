using System;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Configuration;
using IOKode.OpinionatedFramework.Jobs;
using Job = IOKode.OpinionatedFramework.Jobs.Job;

namespace IOKode.OpinionatedFramework.ContractImplementations.TaskRunJobs;

public class TaskRunJobEnqueuer(IConfigurationProvider configuration) : IJobEnqueuer
{
    public Task EnqueueAsync<TJob>(Queue queue, JobCreator<TJob> creator, CancellationToken cancellationToken = default) where TJob : Job
    {
        _ = RetryHelper.RetryOnExceptionAsync(creator, configuration.GetValue<int>("TaskRun:JobEnqueuer:MaxAttempts"));
        return Task.CompletedTask;
    }

    public Task EnqueueWithDelayAsync<TJob>(TimeSpan delay, Queue queue, JobCreator<TJob> creator, CancellationToken cancellationToken = default) where TJob : Job
    {
        _ = Task.Run(async () =>
        {
            await Task.Delay(delay, cancellationToken);

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            await RetryHelper.RetryOnExceptionAsync(creator, configuration.GetValue<int>("TaskRun:JobEnqueuer:MaxAttempts"));
        }, cancellationToken);

        return Task.CompletedTask;
    }
}