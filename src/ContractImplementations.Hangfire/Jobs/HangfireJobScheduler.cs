using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Cronos;
using Hangfire;
using Hangfire.Storage;
using IOKode.OpinionatedFramework.Bootstrapping;
using IOKode.OpinionatedFramework.Jobs;
using Job = IOKode.OpinionatedFramework.Jobs.Job;

namespace IOKode.OpinionatedFramework.ContractImplementations.Hangfire.Jobs;

public class HangfireJobScheduler : IJobScheduler
{
    public Task<Guid> ScheduleAsync<TJob>(CronExpression interval, JobCreator<TJob> creator, CancellationToken cancellationToken = default) where TJob : Job
    {
        // var scheduledJob = new HangfireMutableScheduledJob<TJob>(interval, creator);
        var scheduledJobId = Guid.NewGuid();

        // None cancellation token will be replaced internally.
        // https://docs.hangfire.io/en/latest/background-methods/using-cancellation-tokens.html
        RecurringJob.AddOrUpdate(scheduledJobId.ToString(), () => InvokeJobAsync(creator, CancellationToken.None), interval.ToString);
        return Task.FromResult(scheduledJobId);
    }

    public Task RescheduleAsync(Guid scheduledJobId, CronExpression interval, CancellationToken cancellationToken = default)
    {
        var detailsJob = JobStorage.Current.GetConnection().GetRecurringJobs([scheduledJobId.ToString()]).Single().Job;
        var jobExpression = ReconstructExpressionFromJobData(detailsJob.Method, detailsJob.Args);

        // None cancellation token will be replaced internally.
        // https://docs.hangfire.io/en/latest/background-methods/using-cancellation-tokens.html
        RecurringJob.AddOrUpdate(scheduledJobId.ToString(), jobExpression, interval.ToString);

        return Task.CompletedTask;
    }

    public Task UnscheduleAsync(Guid scheduledJobId, CancellationToken cancellationToken = default)
    {
        RecurringJob.RemoveIfExists(scheduledJobId.ToString());
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
    public static async Task InvokeJobAsync<TJob>(JobCreator<TJob> creator, CancellationToken cancellationToken) where TJob : Job
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

    private static Expression<Action> ReconstructExpressionFromJobData(MethodInfo method, IReadOnlyCollection<object> arguments)
    {
        var parameterExpressions = method.GetParameters()
            .Zip(arguments, (param, arg) => Expression.Constant(arg, param.ParameterType));

        var methodCallExpression = Expression.Call(method, parameterExpressions);
        var lambdaExpression = Expression.Lambda<Action>(methodCallExpression);

        return lambdaExpression;
    }
}