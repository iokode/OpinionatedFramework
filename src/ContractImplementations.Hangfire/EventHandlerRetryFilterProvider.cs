using System;
using System.Collections.Generic;
using System.Linq;
using Hangfire;
using Hangfire.Common;
using IOKode.OpinionatedFramework.ServiceLocation;
using Microsoft.Extensions.DependencyInjection;

namespace IOKode.OpinionatedFramework.ContractImplementations.Hangfire;

/// <summary>
/// Supplies each event handler job the retry policy declared for its handler.
/// </summary>
/// <remarks>
/// Hangfire selects retries with <see cref="AutomaticRetryAttribute"/>, which is normally applied statically to
/// a job method. Every event handler shares one job method here, so a static attribute would set one policy for
/// all of them. A filter provider is Hangfire's own way out: it is asked for the filters of a specific job, so
/// the handler named in that job's arguments decides the attempts. Retries therefore appear in the dashboard as
/// ordinary Hangfire retries.
/// </remarks>
public sealed class EventHandlerRetryFilterProvider : IJobFilterProvider
{
    private static readonly object registrationGate = new();
    private static bool isRegistered;

    /// <summary>
    /// Adds the provider to Hangfire's global provider collection, once per process.
    /// </summary>
    /// <remarks>
    /// The collection is process-wide static, while the container it reads policies from can be rebuilt, so the
    /// provider resolves the declared handlers on every call instead of capturing them here.
    /// </remarks>
    public static void EnsureRegistered()
    {
        lock (registrationGate)
        {
            if (isRegistered)
            {
                return;
            }

            JobFilterProviders.Providers.Add(new EventHandlerRetryFilterProvider());
            isRegistered = true;
        }
    }

    /// <inheritdoc/>
    public IEnumerable<JobFilter> GetFilters(Job job)
    {
        if (job?.Args is null)
        {
            yield break;
        }

        var creator = job.Args.OfType<ExecuteEventHandlerJobCreator>().FirstOrDefault();
        if (creator is null)
        {
            // Not an event handler job, so Hangfire's own defaults apply.
            yield break;
        }

        var policy = FindPolicy(creator);
        if (policy is not {IsRetryConfigured: true})
        {
            yield break;
        }

        var retry = new AutomaticRetryAttribute {Attempts = policy.RetryCount};
        if (policy.RetryDelay is { } delay)
        {
            retry.DelaysInSeconds = [Math.Max(0, (int) delay.TotalSeconds)];
        }

        yield return new JobFilter(retry, JobFilterScope.Method, order: null);
    }

    private static HangfireEventHandlerPolicy? FindPolicy(ExecuteEventHandlerJobCreator creator)
    {
        var options = Locator.ServiceProvider?.GetService<HangfireEventsOptions>();

        return options?.EventHandlerRegistrations
            .FirstOrDefault(registration => registration.HandlerType.FullName == creator.HandlerTypeName)
            ?.Policy;
    }
}
