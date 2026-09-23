using System;
using IOKode.OpinionatedFramework.Events;
using IOKode.OpinionatedFramework.Internals.Events;
using IOKode.OpinionatedFramework.Jobs;
using IOKode.OpinionatedFramework.ServiceContainer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IOKode.OpinionatedFramework.ContractImplementations.Hangfire;

public static class HangfireEventsServiceExtensions
{
    /// <summary>The queue used when the application configures none.</summary>
    public const string DefaultQueueName = "events";

    /// <summary>
    /// Registers the Hangfire-backed event dispatcher and the handlers declared for it.
    /// </summary>
    /// <param name="services">The framework service collection.</param>
    /// <param name="queueName">The queue the handler jobs are enqueued in.</param>
    /// <param name="configuration">Declares the handlers, or <see langword="null"/> to register none.</param>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">Two declared event types share a name.</exception>
    public static void AddHangfireEventDispatcher(this IOpinionatedServiceCollection services,
        string queueName = DefaultQueueName, Action<HangfireEventsOptions>? configuration = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(queueName);

        var options = new HangfireEventsOptions();
        configuration?.Invoke(options);

        var concreteEventTypes = options.ResolveConcreteEventTypes();
        EventNameUniqueness.EnsureUnique(concreteEventTypes);

        // Hangfire resolves filters through a process-wide provider collection, so this is what makes the
        // declared retry policy reach the job.
        EventHandlerRetryFilterProvider.EnsureRegistered();

        foreach (var registration in options.EventHandlerRegistrations)
        {
            services.TryAddTransient(registration.HandlerType);
        }

        services.AddSingleton(options);
        services.AddSingleton(new HangfireEventTypeMap(new EventTypeMap(concreteEventTypes)));
        services.AddTransient<IEventDispatcher>(provider =>
            new HangfireEventDispatcher(options, provider.GetRequiredService<IJobEnqueuer>(), queueName));
    }
}
