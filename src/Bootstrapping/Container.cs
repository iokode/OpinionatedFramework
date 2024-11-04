using System;
using System.Reflection;
using System.Threading;
using IOKode.OpinionatedFramework.Ensuring;
using Microsoft.Extensions.DependencyInjection;

namespace IOKode.OpinionatedFramework.Bootstrapping;

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
        Ensure.Boolean.IsFalse(IsInitialized)
            .ElseThrowsInvalidOperation("The container has already been initialized. It can only be initialized once.");

        _serviceCollection.MakeReadOnly();
        _serviceProvider = _serviceCollection.BuildServiceProvider();
        SetProviderIntoLocator();
    }

    private static void SetProviderIntoLocator()
    {
        var field = typeof(Locator).GetField("_serviceProvider", BindingFlags.Static | BindingFlags.NonPublic)!;
        field.SetValue(null, _serviceProvider);
    }

    private static void SetScopeIntoLocator(IServiceScope? scope)
    {
        var field = typeof(Locator).GetField("_scopedServiceProvider", BindingFlags.NonPublic | BindingFlags.Static)!;
        var providerHolder = (AsyncLocal<IServiceProvider?>)field.GetValue(null)!;
        providerHolder.Value = scope?.ServiceProvider;
    }
}