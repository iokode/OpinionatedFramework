using IOKode.OpinionatedFramework.AspNetCoreIntegrations.CommandControllers.SingleController;
using Microsoft.AspNetCore.Builder;

namespace IOKode.OpinionatedFramework.AspNetCoreIntegrations.CommandControllers;

public static class ApplicationBuilderExtensions
{
    public static void UseCommandControllers(this IApplicationBuilder app, IControllersStrategy? strategy = null)
    {
        strategy ??= new SingleControllerStrategy();
        strategy.Configure(app);
    }
}