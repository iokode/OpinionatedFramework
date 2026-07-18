using System;
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
    public static void AddOpinionatedFramework(this WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        DriverRegistration.RegisterDrivers(builder.Configuration);
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
