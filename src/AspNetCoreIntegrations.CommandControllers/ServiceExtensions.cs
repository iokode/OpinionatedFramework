using System;
using System.Collections.Generic;
using System.Reflection;
using IOKode.OpinionatedFramework.AspNetCoreIntegrations.CommandControllers.SingleController;
using Microsoft.Extensions.DependencyInjection;

namespace IOKode.OpinionatedFramework.AspNetCoreIntegrations.CommandControllers;

public static class ServiceExtensions
{
    public static IServiceCollection AddCommandControllers(this IServiceCollection services, ControllersStrategy? strategy = null)
    {
        strategy ??= new SingleControllerStrategy();

        switch (strategy)
        {
            case SingleControllerStrategy singleControllerStrategy:
                services.AddSingleton(singleControllerStrategy);
                ConfigSingleControllerStrategy(services);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(strategy), strategy, null);
                break;
        }

        return services;
    }

    public static IServiceCollection ScanForControllersCommands(this IServiceCollection services, ICollection<Assembly> assemblies)
    {
        CommandsMapper.RegisterCommandsFromAssemblies(assemblies);
        return services;
    }

    private static void ConfigSingleControllerStrategy(IServiceCollection services)
    {
        services
            .AddControllers()
            .AddApplicationPart(typeof(SingleCommandController).Assembly);
    }
}