using System;
using System.Threading;
using System.Threading.Tasks;
using Cronos;
using Hangfire;
using IOKode.OpinionatedFramework.Bootstrapping;
using IOKode.OpinionatedFramework.Ensuring;
using IOKode.OpinionatedFramework.Jobs;
using Job = IOKode.OpinionatedFramework.Jobs.Job;

namespace IOKode.OpinionatedFramework.ContractImplementations.Hangfire.Jobs;

public class HangfireJobScheduler : IJobScheduler
{
    public Task<ScheduledJob<TJob>> ScheduleAsync<TJob>(CronExpression interval, JobCreator<TJob>? creator, CancellationToken cancellationToken = default) where TJob : Job
    {
        var scheduledJob = new HangfireMutableScheduledJob<TJob>(interval, creator);

        // None cancellation token will be replaced internally.
        // https://docs.hangfire.io/en/latest/background-methods/using-cancellation-tokens.html
        RecurringJob.AddOrUpdate(scheduledJob.Identifier.ToString(), () => InvokeJobAsync(creator.GetJobName(), creator, CancellationToken.None), interval.ToString);
        return Task.FromResult<ScheduledJob<TJob>>(scheduledJob);
    }

    public Task RescheduleAsync<TJob>(ScheduledJob<TJob> scheduledJob, CronExpression interval, CancellationToken cancellationToken = default) where TJob : Job
    {
        Ensure.Type.IsAssignableTo(scheduledJob.GetType(), typeof(MutableScheduledJob<TJob>))
            .ElseThrowsIllegalArgument($"Type must be assignable to {nameof(MutableScheduledJob<TJob>)} type.", nameof(scheduledJob));

        // None cancellation token will be replaced internally.
        // https://docs.hangfire.io/en/latest/background-methods/using-cancellation-tokens.html
        RecurringJob.AddOrUpdate(scheduledJob.Identifier.ToString(), () => InvokeJobAsync(scheduledJob.Creator.GetJobName(), scheduledJob.Creator, CancellationToken.None), interval.ToString);
        ((MutableScheduledJob<TJob>) scheduledJob).ChangeInterval(interval);

        return Task.CompletedTask;
    }

    public Task UnscheduleAsync<TJob>(ScheduledJob<TJob> scheduledJob, CancellationToken cancellationToken = default) where TJob : Job
    {
        RecurringJob.RemoveIfExists(scheduledJob.Identifier.ToString());
        return Task.CompletedTask;
    }

    /// <summary>
    /// This method is not intended to be called directly in application code.
    /// Exists to allow Hangfire to serialize and deserialize the job creator
    /// received on method. This ensures that the job can be processed correctly during execution.
    /// </summary> 
    /// <remarks>
    /// This method must be public because it is used by Hangfire during the deserialization 
    /// and execution of scheduled tasks. Hangfire requires that methods to be invoked 
    /// are publicly accessible to resolve them when deserializing the previously generated expression.
    /// </remarks>
    [JobDisplayName("{0}")]
    public static async Task InvokeJobAsync<TJob>(string jobName, JobCreator<TJob> creator, CancellationToken cancellationToken) where TJob : Job
    {
        try
        {
            Container.Advanced.CreateScope();

            var context = new HangfireJobExecutionContext
            {
                Name = creator.GetJobName(),
                CancellationToken = cancellationToken,
                JobType = typeof(TJob),
                TraceID = Guid.NewGuid(),
            };

            var job = creator.CreateJob();
            await job.ExecuteAsync(context);
        }
        finally
        {
            Container.Advanced.DisposeScope();
        }
    }
}