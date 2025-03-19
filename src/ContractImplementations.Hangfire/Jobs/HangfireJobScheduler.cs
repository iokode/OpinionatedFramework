using System.Threading;
using System.Threading.Tasks;
using Cronos;
using Hangfire;
using IOKode.OpinionatedFramework.Ensuring;
using IOKode.OpinionatedFramework.Facades;
using IOKode.OpinionatedFramework.Jobs;
using Job = IOKode.OpinionatedFramework.Jobs.Job;

namespace IOKode.OpinionatedFramework.ContractImplementations.Hangfire.Jobs;

public class HangfireJobScheduler : IJobScheduler
{
    public Task<ScheduledJob<TJob>> ScheduleAsync<TJob>(CronExpression interval, JobCreator<TJob>? creator, CancellationToken cancellationToken = default) where TJob : Job
    {
        var scheduledJob = new HangfireMutableScheduledJob<TJob>(interval, creator);
        RecurringJob.AddOrUpdate(scheduledJob.Identifier.ToString(), () => InvokeJobAsync(creator), interval.ToString);
        return Task.FromResult<ScheduledJob<TJob>>(scheduledJob);
    }

    public Task RescheduleAsync<TJob>(ScheduledJob<TJob> scheduledJob, CronExpression interval, CancellationToken cancellationToken = default) where TJob : Job
    {
        Ensure.Type.IsAssignableTo(scheduledJob.GetType(), typeof(MutableScheduledJob<TJob>))
            .ElseThrowsIllegalArgument($"Type must be assignable to {nameof(MutableScheduledJob<TJob>)} type.", nameof(scheduledJob));

        RecurringJob.AddOrUpdate(scheduledJob.Identifier.ToString(), () => InvokeJobAsync(scheduledJob.JobArguments), interval.ToString);
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
    public static async Task InvokeJobAsync<TJob>(JobCreator<TJob> creator) where TJob : Job
    {
        var job = creator.CreateJob();
        await job.ExecuteAsync(default);
    }
}