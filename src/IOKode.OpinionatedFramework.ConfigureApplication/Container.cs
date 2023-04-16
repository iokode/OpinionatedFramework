using System;
using System.Reflection;
using IOKode.OpinionatedFramework.Foundation;
using Microsoft.Extensions.DependencyInjection;

namespace IOKode.OpinionatedFramework.ConfigureApplication;

/// <summary>
/// Provides a static container for registering services and building a service provider.
/// </summary>
public static class Container
{
    private static ServiceCollection _serviceCollection = new();
    private static ServiceProvider? _serviceProvider;

    /// <summary>
    /// Gets the service collection used for registering services.
    /// </summary>
    /// <value>
    /// The service collection.
    /// </value>
    public static IServiceCollection Services => _serviceCollection;

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
    public static void Initialize()
    {
        if (IsInitialized)
        {
            throw new InvalidOperationException(
                "The container has already been initialized. It can only be initialized once.");
        }

        _serviceCollection.MakeReadOnly();
        _serviceProvider = _serviceCollection.BuildServiceProvider();
        _setProviderIntoLocator();
    }

    /// <summary>
    /// Clears the current service collection and resets the service provider, effectively allowing the container to be reconfigured.
    /// </summary>
    /// <remarks>
    /// This method should be used with caution, as it will discard all previously registered services and set the service provider to null.
    /// After calling this method, you should re-register your services and call Initialize() again.
    /// This method is primarily intended for use in testing scenarios where the container's state needs to be reset between test runs.
    /// </remarks>
    public static void Clear()
    {
        _serviceCollection = new ServiceCollection();
        _serviceProvider = null;
        _setProviderIntoLocator();
    }

    private static void _setProviderIntoLocator()
    {
        var field = typeof(Locator).GetField("_serviceProvider", BindingFlags.Static | BindingFlags.NonPublic)!;
        field.SetValue(null, _serviceProvider);
    }
}