using System;
using System.Collections.Generic;
using System.Reflection;
using IOKode.OpinionatedFramework.AspNetCoreIntegrations.CommandControllers.SingleController;
using Microsoft.Extensions.DependencyInjection;

namespace IOKode.OpinionatedFramework.AspNetCoreIntegrations.CommandControllers;

public static class ServiceExtensions
{
    public static IServiceCollection AddCommandControllers(this IServiceCollection services, IControllersStrategy? strategy = null)
    {
        strategy ??= new SingleControllerStrategy();
        strategy.ConfigureServices(services);
        return services;       
    }

    public static IServiceCollection ScanForControllersCommands(this IServiceCollection services, ICollection<Assembly> assemblies, Func<Type, string>? commandNameProvider = null)
    {
        CommandsMapper.RegisterCommandsFromAssemblies(assemblies, commandNameProvider);
        return services;
    }

    public static IServiceCollection ScanForControllersJsonConverters(this IServiceCollection services, ICollection<Assembly> assemblies)
    {
        JsonConvertersMapper.RegisterCommandsFromAssemblies(assemblies);
        return services;
    }
}