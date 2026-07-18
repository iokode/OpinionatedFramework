using System;
using IOKode.OpinionatedFramework.HostingIntegrations;
using IOKode.OpinionatedFramework.ServiceContainer;
using IOKode.OpinionatedFramework.ServiceContainer.Drivers;
using Microsoft.Extensions.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace IOKode.OpinionatedFramework.Bootstrapping;

/// <summary>
/// Initializes the framework container or creates and starts a framework host.
/// </summary>
public static class OpinionatedFrameworkBootstrapping
{
    /// <summary>
    /// Initialize the OpinionatedFramework container.
    /// </summary>
    public static void InitializeContainer()
    {
        Container.Initialize();
    }

    /// <summary>Creates and starts an <see cref="IHost"/> using the configured framework drivers.</summary>
    /// <param name="configuration">The host configuration used to select and configure drivers.</param>
    /// <param name="cancellationToken">Cancels host startup.</param>
    /// <returns>A handle that owns the host and initialized container.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="configuration"/> is <see langword="null"/>.</exception>
    /// <exception cref="BootstrapConfigurationException">Driver selection or validation fails.</exception>
    public static async Task<HostHandle> StartAsync(IConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(configurationBuilder => configurationBuilder.AddConfiguration(configuration))
            .ConfigureServices((hostContext, _) => DriverRegistration.RegisterDrivers(hostContext.Configuration))
            .UseServiceProviderFactory(new OpinionatedFrameworkServiceProviderFactory())
            .Build();

        try
        {
            await host.StartAsync(cancellationToken);
            return new HostHandle(host);
        }
        catch
        {
            host.Dispose();
            throw;
        }
    }
}
