using System;
using System.Collections.Generic;
using IOKode.OpinionatedFramework.ServiceContainer;
using Microsoft.Extensions.Configuration;

namespace IOKode.OpinionatedFramework.Bootstrapping.Abstractions;

/// <summary>
/// Provides configuration, service registration, instance identity, and bootstrap-call state to a selected driver.
/// </summary>
public sealed class BootstrapDriverContext
{
    private readonly IDictionary<string, object> sharedState;

    /// <summary>Creates the context supplied to a driver registrar.</summary>
    public BootstrapDriverContext(IOpinionatedServiceCollection services, IConfiguration configuration,
        IConfigurationSection frameworkConfiguration, IConfigurationSection driverConfiguration,
        string? instanceName, IDictionary<string, object> sharedState)
    {
        Services = services;
        Configuration = configuration;
        FrameworkConfiguration = frameworkConfiguration;
        DriverConfiguration = driverConfiguration;
        InstanceName = instanceName;
        this.sharedState = sharedState;
    }

    /// <summary>Gets the service collection to which the selected driver registers its implementation.</summary>
    public IOpinionatedServiceCollection Services { get; }
    /// <summary>Gets the complete host configuration.</summary>
    public IConfiguration Configuration { get; }
    /// <summary>Gets the root <c>OpinionatedFramework</c> configuration section.</summary>
    public IConfigurationSection FrameworkConfiguration { get; }
    /// <summary>Gets the selected driver or named-instance configuration section.</summary>
    public IConfigurationSection DriverConfiguration { get; }
    /// <summary>Gets the configured instance name for named drivers, or <see langword="null"/> for singleton drivers.</summary>
    public string? InstanceName { get; }

    /// <summary>
    /// Gets state shared by all registrar executions within the current bootstrap call, creating it when absent.
    /// </summary>
    /// <remarks>
    /// The state is discarded after bootstrap and is not an application runtime service.
    /// </remarks>
    /// <example>
    /// A registrar selected for multiple contract slots can use shared state to perform common registration once.
    /// A named-instance registrar can use it to coordinate instances that contribute to the same aggregate.
    /// </example>
    /// <typeparam name="TState">The type of bootstrap-only coordination state.</typeparam>
    /// <param name="key">A key unique to the cooperating registrars.</param>
    /// <param name="factory">Creates the state when it has not yet been added.</param>
    /// <returns>The existing or newly created state.</returns>
    public TState GetOrAddSharedState<TState>(string key, Func<TState> factory) where TState : class
    {
        if (this.sharedState.TryGetValue(key, out var state))
        {
            return (TState) state;
        }

        var createdState = factory();
        this.sharedState.Add(key, createdState);
        return createdState;
    }
}
