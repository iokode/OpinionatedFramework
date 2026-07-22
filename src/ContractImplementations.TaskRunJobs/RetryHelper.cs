using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.ServiceContainer;
using IOKode.OpinionatedFramework.Jobs;

namespace IOKode.OpinionatedFramework.ContractImplementations.TaskRunJobs;

internal static class RetryHelper
{
    public static async Task RetryOnExceptionAsync<TJob>(JobCreator<TJob> creator, int maxAttempts,
        CancellationToken cancellationToken = default) where TJob : Job
    {
        bool shouldRetry = false;
        int attempt = 0;
        var exceptions = new List<Exception>();

        do
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                attempt++;

                var context = new JobContext
                {
                    CancellationToken = cancellationToken,
                    Name = creator.GetJobName(),
                    JobType = typeof(TJob),
                    TraceID = Guid.NewGuid()
                };

                await using var scope = Container.Advanced.CreateScope();
                await creator.CreateJob().ExecuteAsync(context);
                shouldRetry = false;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);

                if (attempt < maxAttempts)
                {
                    shouldRetry = true;
                }
                else
                {
                    throw new AggregateException(exceptions);
                }
            }
        } while (shouldRetry);
    }
}
