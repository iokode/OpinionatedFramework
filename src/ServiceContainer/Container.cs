using System;
using IOKode.OpinionatedFramework.ServiceLocation;
using Microsoft.Extensions.DependencyInjection;

namespace IOKode.OpinionatedFramework.ServiceContainer;

/// <summary>
/// Provides a static container for registering services and building a service provider.
/// </summary>
public static partial class Container
{
    private static OpinionatedServiceCollection _serviceCollection = new();
    private static ServiceProvider? _serviceProvider;
    private static IServiceScope? _serviceProviderScope;

    /// <summary>
    /// Gets the service collection used for registering services.
    /// </summary>
    /// <value>
    /// The service collection.
    /// </value>
    public static IOpinionatedServiceCollection Services => _serviceCollection;

    /// <summary>
    /// Gets a value indicating whether the container has been initialized.
    /// </summary>
    /// <value>
    /// true if the container is initialized; otherwise, false.
    /// </value>
    public static bool IsInitialized => _serviceProvider != null;

    /// <summary>
    /// Initializes the container by making the service collection read-only and building the service provider.
    /// </summary>
    /// <remarks>
    /// Call this method after registering all services in the service collection.
    /// </remarks>
    /// <exception cref="InvalidOperationException">The container is already initialized.</exception>
    public static void Initialize()
    {
        if (IsInitialized)
        {
            throw new InvalidOperationException("The container has already been initialized. It can only be initialized once.");
        }

        _serviceCollection.MakeReadOnly();
        _serviceProvider = _serviceCollection.BuildServiceProvider();
        SetProviderIntoLocator();
    }

    private static void SetProviderIntoLocator()
    {
        Locator.SetRootServiceProvider(_serviceProvider);
    }

    private static void SetScopeIntoLocator(IServiceScope? scope)
    {
        Locator.SetScopedServiceProvider(scope?.ServiceProvider);
    }
}
