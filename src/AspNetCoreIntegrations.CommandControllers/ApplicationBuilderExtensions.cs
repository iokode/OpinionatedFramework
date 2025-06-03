using System;
using IOKode.OpinionatedFramework.AspNetCoreIntegrations.CommandControllers.SingleController;
using Microsoft.AspNetCore.Builder;

namespace IOKode.OpinionatedFramework.AspNetCoreIntegrations.CommandControllers;

public static class ApplicationBuilderExtensions
{
    public static void AddCommandControllers(this WebApplication app, ControllersStrategy? strategy = null)
    {
        strategy ??= new SingleControllerStrategy();

        switch (strategy)
        {
            case SingleControllerStrategy singleControllerStrategy:
                ConfigSingleControllerStrategy(app, singleControllerStrategy);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(strategy), strategy, null);
                break;
        }
    }

    private static void ConfigSingleControllerStrategy(WebApplication app, SingleControllerStrategy strategy)
    {
        var cleanRoute = strategy.Route.TrimStart('/');

        app.UseRouting();
        app.MapControllerRoute
        (
            name: "CommandRoute",
            pattern: $"/{cleanRoute}",
            defaults: new
            {
                controller = "SingleCommand",
                action = nameof(SingleCommandController.InvokeCommand)
            }
        );
    }
    
    public static void UseCommandControllers(this IApplicationBuilder app, ControllersStrategy? strategy = null)
    {
        strategy ??= new SingleControllerStrategy();

        switch (strategy)
        {
            case SingleControllerStrategy singleControllerStrategy:
                ConfigSingleControllerStrategy(app, singleControllerStrategy);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(strategy), strategy, null);
        }
    }

    private static void ConfigSingleControllerStrategy(IApplicationBuilder app, SingleControllerStrategy strategy)
    {
        var cleanRoute = strategy.Route.TrimStart('/');
        
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute
            (
                name: "CommandRoute",
                pattern: $"/{cleanRoute}",
                defaults: new
                {
                    controller = "SingleCommand",
                    action = "InvokeCommand"
                }
            );
        });
    }
}