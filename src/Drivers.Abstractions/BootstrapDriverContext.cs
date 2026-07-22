using System;
using System.Collections.Generic;
using IOKode.OpinionatedFramework.ServiceContainer;
using Microsoft.Extensions.Configuration;

namespace IOKode.OpinionatedFramework.Drivers.Abstractions;

/// <summary>
/// Provides configuration, service registration, instance identity, and bootstrap-call state to a selected driver.
/// </summary>
public sealed class BootstrapDriverContext
{
    private readonly IDictionary<string, object> sharedState;
    private readonly BootstrapOptions options;

    /// <summary>Creates the context supplied to a driver registrar.</summary>
    public BootstrapDriverContext(IOpinionatedServiceCollection services, IConfiguration configuration,
        IConfigurationSection frameworkConfiguration, IConfigurationSection driverConfiguration,
        string? instanceName, IDictionary<string, object> sharedState, BootstrapOptions? options = null)
    {
        Services = services;
        Configuration = configuration;
        FrameworkConfiguration = frameworkConfiguration;
        DriverConfiguration = driverConfiguration;
        InstanceName = instanceName;
        this.sharedState = sharedState;
        this.options = options ?? new BootstrapOptions();
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
    /// Gets the application-supplied configuration for the calling driver's options type.
    /// </summary>
    /// <remarks>
    /// A driver calls this once per bootstrap even when it expects no configuration, so that application
    /// configuration no selected driver consumes is reported instead of silently discarded. The returned
    /// delegate is applied on top of the values read from configuration.
    /// </remarks>
    /// <typeparam name="TOptions">The options type owned by the calling driver.</typeparam>
    /// <returns>
    /// A delegate applying every configurator the application added, or <see langword="null"/> when it added none.
    /// </returns>
    public Action<TOptions>? GetOptionsConfigurator<TOptions>()
    {
        return this.options.Consume<TOptions>();
    }

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
