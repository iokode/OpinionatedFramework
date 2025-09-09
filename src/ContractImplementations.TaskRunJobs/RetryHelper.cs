using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Bootstrapping;
using IOKode.OpinionatedFramework.Jobs;

namespace IOKode.OpinionatedFramework.ContractImplementations.TaskRunJobs;

internal static class RetryHelper
{
    public static async Task RetryOnExceptionAsync<TJob>(JobCreator<TJob> creator, int maxAttempts) where TJob : Job
    {
        bool shouldRetry = false;
        int attempt = 0;
        var exceptions = new List<Exception>();

        do
        {
            try
            {
                attempt++;

                var context = new JobContext
                {
                    CancellationToken = CancellationToken.None,
                    Name = creator.GetJobName(),
                    JobType = typeof(TJob),
                    TraceID = Guid.NewGuid()
                };

                Container.Advanced.CreateScope();
                await creator.CreateJob().ExecuteAsync(context);
                shouldRetry = false;
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
            finally
            {
                Container.Advanced.DisposeScope();
            }
        } while (shouldRetry);
    }
}