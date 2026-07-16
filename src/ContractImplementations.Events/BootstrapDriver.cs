using IOKode.OpinionatedFramework.Bootstrapping;
using IOKode.OpinionatedFramework.Bootstrapping.Abstractions;
using IOKode.OpinionatedFramework.Events;

[assembly: BootstrapDriver<IEventDispatcher,
    IOKode.OpinionatedFramework.ContractImplementations.Events.EventDispatcherBootstrapDriver>("Events", "default")]

namespace IOKode.OpinionatedFramework.ContractImplementations.Events;

public sealed class EventDispatcherBootstrapDriver : IBootstrapDriverRegistrar
{
    public static BootstrapValidationResult Validate(BootstrapDriverContext context)
    {
        return BootstrapValidationResult.Success;
    }

    public static void Register(BootstrapDriverContext context)
    {
        context.Services.AddEventDispatcher();
    }
}
