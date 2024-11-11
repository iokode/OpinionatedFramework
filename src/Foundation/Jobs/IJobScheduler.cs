using System;
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
    /// Schedules a job to run at intervals specified by a <see cref="CronExpression"/>.
    /// </summary>
    /// <param name="job">The job to be scheduled.</param>
    /// <param name="interval">The cron expression representing the interval for the job to run.</param>
    /// <param name="cancellationToken">A token to cancel the scheduling process, but it does NOT cancel the job once it is scheduled.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation of scheduling. The task result contains a <see cref="ScheduledJob"/> representing the scheduled job.</returns>
    Task<ScheduledJob> ScheduleAsync(IJob job, CronExpression interval, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reschedules an existing scheduled job to run at a new interval.
    /// </summary>
    /// <param name="scheduledJob">The job to be rescheduled.</param>
    /// <param name="interval">The new cron expression representing the interval for the job to run.</param>
    /// <param name="cancellationToken">A token to cancel the rescheduling process, but it does NOT cancel the job itself once it is rescheduled.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation of rescheduling.</returns>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="scheduledJob"/> was created with a different
    ///     <see cref="IJobScheduler"/> implementation than the one being used to reschedule it, or if it was not generated
    ///     by the <see cref="ScheduleAsync"/> method (e.g., manually instantiated).</exception>
    Task RescheduleAsync(ScheduledJob scheduledJob, CronExpression interval, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unschedules an existing scheduled job.
    /// </summary>
    /// <param name="scheduledJob">The job to be unscheduled.</param>
    /// <param name="cancellationToken">A token to cancel the unscheduling process, but it does NOT cancel the job itself if it is already running.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation of unscheduling.</returns>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="scheduledJob"/> was created with a different
    ///     <see cref="IJobScheduler"/> implementation than the one being used to unschedule it, or if it was not generated
    ///     by the <see cref="ScheduleAsync"/> method (e.g., manually instantiated).</exception>
    Task UnscheduleAsync(ScheduledJob scheduledJob, CancellationToken cancellationToken = default);
}