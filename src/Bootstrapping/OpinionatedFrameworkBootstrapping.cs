using System;
using IOKode.OpinionatedFramework.Drivers.Abstractions;
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
    /// <param name="configureOptions">
    /// Supplies the code-level driver configuration that cannot be expressed in <paramref name="configuration"/>,
    /// such as a command middleware selected by type. Each referenced driver package contributes its own
    /// extension methods over <see cref="IBootstrapOptions"/>.
    /// </param>
    /// <param name="cancellationToken">Cancels host startup.</param>
    /// <returns>A handle that owns the host and initialized container.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="configuration"/> is <see langword="null"/>.</exception>
    /// <exception cref="BootstrapConfigurationException">
    /// Driver selection or validation fails, or no selected driver consumes the supplied options.
    /// </exception>
    public static async Task<HostHandle> StartAsync(IConfiguration configuration,
        Action<IBootstrapOptions>? configureOptions = null, CancellationToken cancellationToken = default)
    {
        var options = new BootstrapOptions();
        configureOptions?.Invoke(options);
        return await StartHostAsync(configuration, options, cancellationToken);
    }

    /// <summary>Creates and starts an <see cref="IHost"/> without registering framework drivers.</summary>
    /// <param name="configuration">The configuration added to the host.</param>
    /// <param name="cancellationToken">Cancels host startup.</param>
    /// <returns>A handle that owns the host and initialized container.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="configuration"/> is <see langword="null"/>.</exception>
    public static async Task<HostHandle> StartWithoutDriversAsync(IConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        return await StartHostAsync(configuration, null, cancellationToken);
    }

    private static async Task<HostHandle> StartHostAsync(IConfiguration configuration, BootstrapOptions? options,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var hostBuilder = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(configurationBuilder => configurationBuilder.AddConfiguration(configuration));
        if (options is not null)
        {
            hostBuilder.ConfigureServices((hostContext, _) =>
                DriverRegistration.RegisterDrivers(hostContext.Configuration, options));
        }

        var host = hostBuilder.UseServiceProviderFactory(new OpinionatedFrameworkServiceProviderFactory()).Build();

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
