using IOKode.OpinionatedFramework.AspNetCoreIntegrations.ControllerGenerators.SingleController;
using Microsoft.AspNetCore.Builder;

namespace IOKode.OpinionatedFramework.AspNetCoreIntegrations.ControllerGenerators;

public static class ApplicationBuilderExtensions
{
    public static void UseGeneratedControllersForCommands(this IApplicationBuilder app, IControllersStrategy? strategy = null)
    {
        strategy ??= new SingleControllerStrategy();
        strategy.Configure(app);
    }
}