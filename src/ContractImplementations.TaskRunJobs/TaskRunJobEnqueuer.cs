using System;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Configuration;
using IOKode.OpinionatedFramework.Jobs;

namespace IOKode.OpinionatedFramework.ContractImplementations.TaskRunJobs;

public class TaskRunJobEnqueuer(IConfigurationProvider configuration) : IJobEnqueuer
{
    public Task EnqueueAsync(Queue queue, IJob job, CancellationToken cancellationToken)
    {
        _ = RetryHelper.RetryOnExceptionAsync(job, configuration.GetValue<int>("TaskRun:JobEnqueuer:MaxAttempts"));
        return Task.CompletedTask;
    }

    public Task EnqueueWithDelayAsync(Queue queue, IJob job, TimeSpan delay, CancellationToken cancellationToken)
    {
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