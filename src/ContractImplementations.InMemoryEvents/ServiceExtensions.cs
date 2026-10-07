using System;
using IOKode.OpinionatedFramework.Events;
using IOKode.OpinionatedFramework.Internals.Events;
using IOKode.OpinionatedFramework.ServiceContainer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IOKode.OpinionatedFramework.ContractImplementations.InMemoryEvents;

public static class ServiceExtensions
{
    /// <summary>
    /// Registers the in-memory event dispatcher and the handlers declared for it.
    /// </summary>
    /// <param name="services">The framework service collection.</param>
    /// <param name="configuration">Declares the handlers, or <see langword="null"/> to register none.</param>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    /// <exception cref="DuplicateEventNameException">Two declared event types share a name.</exception>
    public static void AddInMemoryEventDispatcher(this IOpinionatedServiceCollection services,
        Action<InMemoryEventsOptions>? configuration = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new InMemoryEventsOptions();
        configuration?.Invoke(options);
        EventNameUniqueness.EnsureUnique(options.DeclaredEventTypes);

        // Handlers are ordinary services, so they get constructor injection instead of being activated by the
        // dispatcher, and a handler can depend on whatever the scope it runs in provides.
        foreach (var registration in options.EventHandlerRegistrations)
        {
            services.TryAddTransient(registration.HandlerType);
        }

        services.AddSingleton(options);

        // One singleton registration and no second one delegating to it: the container tracks what it creates
        // for disposal, so registering the same instance twice would dispose it twice.
        services.AddSingleton<IEventDispatcher, InMemoryEventDispatcher>();
    }
}
