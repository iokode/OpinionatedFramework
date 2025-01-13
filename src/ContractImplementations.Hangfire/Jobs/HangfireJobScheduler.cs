using System.Threading;
using System.Threading.Tasks;
using Cronos;
using Hangfire;
using IOKode.OpinionatedFramework.Ensuring;
using IOKode.OpinionatedFramework.Jobs;

namespace IOKode.OpinionatedFramework.ContractImplementations.Hangfire.Jobs;

public class HangfireJobScheduler : IJobScheduler
{
    public Task<ScheduledJob> ScheduleAsync(IJob job, CronExpression interval, CancellationToken cancellationToken)
    {
        var scheduledJob = new HangfireMutableScheduledJob(interval, job);
        RecurringJob.AddOrUpdate(scheduledJob.Identifier.ToString(), () => InvokeAsync(job), interval.ToString);
        return Task.FromResult<ScheduledJob>(scheduledJob);
    }

    public Task RescheduleAsync(ScheduledJob scheduledJob, CronExpression interval, CancellationToken cancellationToken)
    {
        Ensure.Type.IsAssignableTo(scheduledJob.GetType(), typeof(MutableScheduledJob))
            .ElseThrowsIllegalArgument($"Type must be assignable to {nameof(MutableScheduledJob)} type.", nameof(scheduledJob));

        RecurringJob.AddOrUpdate(scheduledJob.Identifier.ToString(), () => InvokeAsync(scheduledJob.Job), interval.ToString);
        ((MutableScheduledJob) scheduledJob).ChangeInterval(interval);

        return Task.CompletedTask;
    }

    public Task UnscheduleAsync(ScheduledJob scheduledJob, CancellationToken cancellationToken)
    {
        RecurringJob.RemoveIfExists(scheduledJob.Identifier.ToString());
        return Task.CompletedTask;
    }

    /// <summary>
    /// This method is not intended to be called directly in application code.
    /// Exists to allow Hangfire to serialize and deserialize the job object 
    /// that it will receive as an argument. This ensures that the job 
    /// can be processed correctly during execution.
    /// </summary> 
    /// <remarks>
    /// This method must be public because it is used by Hangfire during the deserialization 
    /// and execution of scheduled tasks. Hangfire requires that methods to be invoked 
    /// are publicly accessible to resolve them when deserializing the previously generated expression.
    /// </remarks>
    public static async Task InvokeAsync(IJob job)
    {
        await job.InvokeAsync(default);
    }
}