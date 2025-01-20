using IOKode.OpinionatedFramework.Bootstrapping;
using IOKode.OpinionatedFramework.Events;
using Microsoft.Extensions.DependencyInjection;

namespace IOKode.OpinionatedFramework.ContractImplementations.Events;

public static class ServiceExtensions
{
    public static void AddEventDispatcher(this IOpinionatedServiceCollection services)
    {
        services.AddTransient<IEventDispatcher, EventDispatcher>();
    }
}