using IOKode.OpinionatedFramework.Drivers.Abstractions;
using IOKode.OpinionatedFramework.Events;
using IOKode.OpinionatedFramework.Internals.Events;

[assembly: BootstrapDriver<IEventDispatcher,
    IOKode.OpinionatedFramework.ContractImplementations.InMemoryEvents.InMemoryEventDispatcherBootstrapDriver>("Events", "InMemory", true)]

namespace IOKode.OpinionatedFramework.ContractImplementations.InMemoryEvents;

public sealed class InMemoryEventDispatcherBootstrapDriver : IBootstrapDriverRegistrar
{
    public static BootstrapValidationResult Validate(BootstrapDriverContext context)
    {
        var options = new InMemoryEventsOptions();
        context.GetOptionsConfigurator<InMemoryEventsOptions>()?.Invoke(options);

        return new BootstrapValidationResult(
            EventNameUniqueness.Validate(options.DeclaredEventTypes, context.DriverConfiguration.Path));
    }

    public static void Register(BootstrapDriverContext context)
    {
        context.Services.AddInMemoryEventDispatcher(context.GetOptionsConfigurator<InMemoryEventsOptions>());
    }
}
