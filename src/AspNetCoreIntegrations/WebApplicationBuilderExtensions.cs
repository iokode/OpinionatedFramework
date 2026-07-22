using System;
using IOKode.OpinionatedFramework.Drivers.Abstractions;
using IOKode.OpinionatedFramework.HostingIntegrations;
using IOKode.OpinionatedFramework.ServiceContainer.Drivers;
using Microsoft.AspNetCore.Builder;

namespace IOKode.OpinionatedFramework.AspNetCoreIntegrations;

/// <summary>Provides OpinionatedFramework bootstrapping for ASP.NET Core hosts.</summary>
public static class WebApplicationBuilderExtensions
{
    /// <summary>
    /// Selects and registers configured drivers, unifies the framework and ASP.NET Core containers,
    /// and enables host lifecycle handling.
    /// </summary>
    /// <param name="builder">The ASP.NET Core application builder.</param>
    /// <param name="configureOptions">
    /// Supplies the code-level driver configuration that cannot be expressed in configuration, such as a command
    /// middleware selected by type. Each referenced driver package contributes its own extension methods over
    /// <see cref="IBootstrapOptions"/>.
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> is <see langword="null"/>.</exception>
    /// <exception cref="BootstrapConfigurationException">
    /// Driver selection or validation fails, or no selected driver consumes the supplied options.
    /// </exception>
    public static void AddOpinionatedFramework(this WebApplicationBuilder builder,
        Action<IBootstrapOptions>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var options = new BootstrapOptions();
        configureOptions?.Invoke(options);

        DriverRegistration.RegisterDrivers(builder.Configuration, options);
        builder.AddOpinionatedFrameworkWithoutDrivers();
    }

    /// <summary>
    /// Configures ASP.NET Core to use the OpinionatedFramework service provider without registering drivers.
    /// </summary>
    /// <param name="builder">The ASP.NET Core application builder.</param>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> is <see langword="null"/>.</exception>
    public static void AddOpinionatedFrameworkWithoutDrivers(this WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Host.UseServiceProviderFactory(new OpinionatedFrameworkServiceProviderFactory());
    }
}
