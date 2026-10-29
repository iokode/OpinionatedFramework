using System;
using System.Linq;
using System.Threading;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;
using IOKode.OpinionatedFramework.Events;
using IOKode.OpinionatedFramework.Internals.Events;
using IOKode.OpinionatedFramework.Logging;
using IOKode.OpinionatedFramework.ServiceLocation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IOKode.OpinionatedFramework.ContractImplementations.Hangfire;

/// <summary>
/// Raises <see cref="EventHandlerExhausted"/> when an event handler job is left failed.
/// </summary>
/// <remarks>
/// Hangfire's retry filter turns every failure into a scheduled retry while attempts remain, so a job reaches the
/// failed state only once they are exhausted. That is the state this filter waits for, which makes the event
/// follow whatever retry applied, the declared policy or Hangfire's own default.
/// </remarks>
/// <remarks>
/// The attempts come from the retry count Hangfire keeps with the job, not from the policy, because a handler
/// without a declared policy gets Hangfire's default. A failed job requeued by hand that fails again reaches the
/// failed state again, and raises the event again.
/// </remarks>
/// <remarks>
/// The event counts the attempts of the round that was exhausted, not every attempt the job ever made. Hangfire
/// does not reset its retry count when a job is requeued, so the filter keeps the count each exhaustion left and
/// subtracts it from the next one. A requeued job that has used up its retries gets a single attempt, and its
/// exhaustion reports one.
/// </remarks>
/// <remarks>
/// A job stopped by a server shutting down is not failed but requeued by Hangfire, so a cancellation never
/// reaches this filter.
/// </remarks>
public sealed class EventHandlerExhaustionFilter : IApplyStateFilter
{
    /// <summary>The job parameter Hangfire's retry filter counts the retries in.</summary>
    private const string RetryCountParameter = "RetryCount";

    /// <summary>The job parameter this filter keeps the retry count of the last exhaustion in.</summary>
    private const string ExhaustedRetryCountParameter = "OpinionatedFramework.ExhaustedRetryCount";

    private static readonly Lock registrationGate = new();
    private static bool isRegistered;

    /// <summary>
    /// Adds the filter to Hangfire's global filters, once per process.
    /// </summary>
    /// <remarks>
    /// The collection is process-wide static, while the container it reads the declarations from can be rebuilt,
    /// so the filter resolves them on every call instead of capturing them here.
    /// </remarks>
    public static void EnsureRegistered()
    {
        lock (registrationGate)
        {
            if (isRegistered)
            {
                return;
            }

            GlobalJobFilters.Filters.Add(new EventHandlerExhaustionFilter());
            isRegistered = true;
        }
    }

    /// <inheritdoc/>
    public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        if (context.NewState is not FailedState)
        {
            return;
        }

        var creator = context.BackgroundJob.Job?.Args?.OfType<ExecuteEventHandlerJobCreator>().FirstOrDefault();
        if (creator is null)
        {
            // Not an event handler job.
            return;
        }

        var serviceProvider = Locator.ServiceProvider;
        if (serviceProvider is null)
        {
            return;
        }

        try
        {
            // Kept whether or not the event is raised, so a later round is counted from where this one ended.
            int attempts = CountAttemptsOfThisRound(context);

            var options = serviceProvider.GetService<HangfireEventsOptions>();
            if (options is null || !EventHandlerExhaustion.IsRaisedFor(options, creator.EventName))
            {
                return;
            }

            var exhausted = EventHandlerExhaustion.Describe(
                creator.EventName,
                creator.EventBody,
                creator.HandlerTypeName,
                attempts);

            // Hangfire filters are synchronous. The dispatcher only stores jobs, through a connection of its own,
            // so waiting for it here holds nothing the state transition needs.
            serviceProvider.GetRequiredService<IEventDispatcher>()
                .DispatchAsync(exhausted, CancellationToken.None)
                .GetAwaiter()
                .GetResult();
        }
        catch (Exception exception)
        {
            // Letting it escape would fail the state transition of the job, which has failed regardless.
            serviceProvider.GetService<ILogging>()?.FromCategory<EventHandlerExhaustionFilter>().LogError(
                exception,
                "The handler {Handler} exhausted its attempts on the event {Event}, and raising {Exhausted} failed.",
                creator.HandlerTypeName,
                creator.EventName,
                EventName.Of<EventHandlerExhausted>());
        }
    }

    /// <inheritdoc/>
    public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
    }

    private static int CountAttemptsOfThisRound(ApplyStateContext context)
    {
        // Absent until Hangfire schedules the first retry, which a handler without retries never gets.
        int retryCount = ReadParameter(context, RetryCountParameter);

        // Absent until the job is exhausted for the first time.
        int exhaustedRetryCount = ReadParameter(context, ExhaustedRetryCountParameter);

        context.Connection.SetJobParameter(
            context.BackgroundJob.Id,
            ExhaustedRetryCountParameter,
            SerializationHelper.Serialize(retryCount));

        return retryCount - exhaustedRetryCount + 1;
    }

    private static int ReadParameter(ApplyStateContext context, string name)
    {
        var value = context.Connection.GetJobParameter(context.BackgroundJob.Id, name);
        return value is null ? 0 : SerializationHelper.Deserialize<int>(value);
    }
}
