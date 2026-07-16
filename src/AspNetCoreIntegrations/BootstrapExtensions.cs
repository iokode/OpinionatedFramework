using System;
using IOKode.OpinionatedFramework.Bootstrapping;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace IOKode.OpinionatedFramework.AspNetCoreIntegrations;

/// <summary>Provides OpinionatedFramework bootstrapping for ASP.NET Core hosts.</summary>
public static class BootstrapExtensions
{
    /// <summary>
    /// Selects and registers configured drivers, unifies the framework and ASP.NET Core containers,
    /// and adds hosted lifecycle handling.
    /// </summary>
    public static void AddOpinionatedFramework(this WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        OpinionatedFrameworkBootstrapping.RegisterDrivers(builder.Configuration);
        builder.Host.UseServiceProviderFactory(new OpinionatedFrameworkServiceProviderFactory());
        builder.Services.AddHostedService<LifecycleHostedService>();
    }
}
