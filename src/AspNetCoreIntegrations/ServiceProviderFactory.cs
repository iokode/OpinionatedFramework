using System;
using IOKode.OpinionatedFramework.Bootstrapping;
using Microsoft.Extensions.DependencyInjection;

namespace IOKode.OpinionatedFramework.AspNetCoreIntegrations;

/// <summary>
/// A custom implementation of <see cref="IServiceProviderFactory{TContainerBuilder}"/> designed for
/// integrating the OpinionatedFramework's Container into the dependency injection system of an application.
/// </summary>
/// <remarks>
/// The <see cref="OpinionatedFrameworkServiceProviderFactory"/> is responsible for performing the
/// initial configuration and bootstrapping of the OpinionatedFramework. This includes adding services
/// to the global <see cref="Container"/> and initializing the container through <see cref="Container.Initialize"/>.
/// </remarks>
public class OpinionatedFrameworkServiceProviderFactory : IServiceProviderFactory<IServiceCollection>
{
    public IServiceCollection CreateBuilder(IServiceCollection services)
    {
        return services;
    }

    public IServiceProvider CreateServiceProvider(IServiceCollection services)
    {
        foreach (var service in services)
        {
            Container.Services.Add(service);
        }

        Container.Initialize();
        return Locator.ServiceProvider!;
    }
}