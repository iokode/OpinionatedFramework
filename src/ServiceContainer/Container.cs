using System;
using IOKode.OpinionatedFramework.ServiceLocation;
using Microsoft.Extensions.DependencyInjection;

namespace IOKode.OpinionatedFramework.ServiceContainer;

/// <summary>Provides a static container for registering services and building a service provider.</summary>
public static partial class Container
{
    private static OpinionatedServiceCollection _serviceCollection = new();
    private static bool _isDisposed;

    /// <summary>Gets the service collection used for registering services.</summary>
    public static IOpinionatedServiceCollection Services => _serviceCollection;

    /// <summary>Gets whether the root service provider has been initialized.</summary>
    public static bool IsInitialized => Locator.RootServiceProvider is not null;

    /// <summary>Gets whether the container has been disposed.</summary>
    public static bool IsDisposed => _isDisposed;

    /// <summary>Builds and registers the root service provider.</summary>
    /// <exception cref="InvalidOperationException">The container is disposed or already initialized.</exception>
    public static void Initialize()
    {
        EnsureNotDisposed();

        if (IsInitialized)
        {
            throw new InvalidOperationException("The container has already been initialized. It can only be initialized once.");
        }

        _serviceCollection.MakeReadOnly();
        Locator.SetRootServiceProvider(_serviceCollection.BuildServiceProvider());
    }

    private static void EnsureInitialized()
    {
        if (!IsInitialized)
        {
            throw new InvalidOperationException("The container has not been initialized. Call 'Container.Initialize()' method.");
        }
    }

    private static void EnsureNotDisposed()
    {
        if (IsDisposed)
        {
            throw new InvalidOperationException("The container is disposed.");
        }
    }
    
    private static void EnsureInitializedAndNotDisposed()
    {
        EnsureInitialized();
        EnsureNotDisposed();
    }
}
