using System.Threading;
using System.Threading.Tasks;
using Cronos;
using IOKode.OpinionatedFramework.Facades;

namespace IOKode.OpinionatedFramework.Jobs;

/// <summary>
/// Defines a job scheduler contract that allows scheduling, rescheduling, and unscheduling jobs based on a cron expression.
/// </summary>
[AddToFacade("Job")]
public interface IJobScheduler
{
    /// <summary>
    /// Schedules a job to run at the specified interval.
    /// </summary>
    /// <typeparam name="TJob">The type of the job to be scheduled, which must implement <see cref="IJob"/>.</typeparam>
    /// <param name="interval">The cron expression representing the schedule interval for the job.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation, if needed.</param>
    /// <returns>
    /// A task representing the asynchronous operation of scheduling the job.
    /// The result contains details of the scheduled job.
    /// </returns>
    Task<ScheduledJob<TJob>> ScheduleAsync<TJob>(CronExpression interval, CancellationToken cancellationToken = default) where TJob : IJob
    {
        return ScheduleAsync<TJob>(interval, null, cancellationToken);
    }

    /// <summary>
    /// Schedules a job to run at the specified interval, providing optional arguments for the job.
    /// </summary>
    /// <typeparam name="TJob">The type of the job to be scheduled, which must implement <see cref="IJob"/>.</typeparam>
    /// <param name="interval">The cron expression representing the schedule interval for the job.</param>
    /// <param name="jobArguments">Optional arguments for creating the job instance. If null, a default instance of the job will be used.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation, if needed.</param>
    /// <returns>
    /// A task representing the asynchronous operation of enqueuing the job.
    /// The result contains details of the scheduled job.
    /// </returns>
    Task<ScheduledJob<TJob>> ScheduleAsync<TJob>(CronExpression interval, JobArguments<TJob>? jobArguments, CancellationToken cancellationToken = default) where TJob : IJob;

    /// <summary>
    /// Reschedules an already scheduled job to run at a new specified interval.
    /// </summary>
    /// <typeparam name="TJob">The type of the job to be rescheduled, which must implement <see cref="IJob"/>.</typeparam>
    /// <param name="scheduledJob">The job that has already been scheduled and needs rescheduling.</param>
    /// <param name="interval">The new cron expression representing the updated schedule interval for the job.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation, if needed.</param>
    /// <returns>A task representing the asynchronous operation of rescheduling the job.</returns>
    Task RescheduleAsync<TJob>(ScheduledJob<TJob> scheduledJob, CronExpression interval, CancellationToken cancellationToken = default) where TJob : IJob;

    /// <summary>
    /// Unschedules a previously scheduled job, effectively removing it from the schedule.
    /// </summary>
    /// <typeparam name="TJob">The type of the job to be unscheduled, which must implement <see cref="IJob"/>.</typeparam>
    /// <param name="scheduledJob">The instance of the previously scheduled job to be unscheduled.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation, if needed.</param>
    /// <returns>A task representing the asynchronous operation of unscheduling the job.</returns>
    Task UnscheduleAsync<TJob>(ScheduledJob<TJob> scheduledJob, CancellationToken cancellationToken = default) where TJob : IJob;
}