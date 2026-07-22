using System;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.ServiceContainer;
using IOKode.OpinionatedFramework.ServiceLocation;
using Microsoft.Extensions.DependencyInjection;

namespace IOKode.OpinionatedFramework.HostingIntegrations;

/// <summary>
/// Combines Microsoft.Extensions.Hosting service registrations with the OpinionatedFramework container.
/// </summary>
public sealed class OpinionatedFrameworkServiceProviderFactory : IServiceProviderFactory<IServiceCollection>
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
        return new ContainerServiceProvider(Locator.ServiceProvider!);
    }

    private sealed class ContainerServiceProvider(IServiceProvider serviceProvider) : IServiceProvider, IAsyncDisposable
    {
        public object? GetService(Type serviceType)
        {
            return serviceProvider.GetService(serviceType);
        }

        public async ValueTask DisposeAsync()
        {
            if (!Container.IsDisposed)
            {
                await Container.Advanced.DisposeAsync();
            }
        }
    }
}
