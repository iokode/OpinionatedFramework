using System;
using System.Reflection;
using System.Threading;
using IOKode.OpinionatedFramework.Ensuring;
using Microsoft.Extensions.DependencyInjection;

namespace IOKode.OpinionatedFramework.ConfigureApplication;

/// <summary>
/// Provides a static container for registering services and building a service provider.
/// </summary>
public static class Container
{
    private static ServiceCollection _serviceCollection = new();
    private static ServiceProvider? _serviceProvider;
    private static IServiceScope? _serviceProviderScope;

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
    /// <exception cref="InvalidOperationException">The container is already initialized.</exception>
    public static void Initialize()
    {
        Ensure.Boolean.IsFalse(IsInitialized)
            .ElseThrowsInvalidOperation("The container has already been initialized. It can only be initialized once.");

        _serviceCollection.MakeReadOnly();
        _serviceProvider = _serviceCollection.BuildServiceProvider();
        SetProviderIntoLocator();
    }

    /// <summary>
    /// Creates a new service scope that can be used to resolve scoped services and sets it in the Locator.
    /// </summary>
    /// <returns>A new IServiceScope that can be used to resolve scoped services.</returns>
    /// <exception cref="InvalidOperationException">Thrown when trying to create a scope before the container is initialized.</exception>
    /// <remarks>
    /// Scoped services are disposed when the scope is disposed.This method returns an IServiceScope which should
    /// be disposed by the caller, typically in a using block.
    ///
    /// Typically, you shouldn't call this method directly, except in advanced scenarios like creating a custom
    /// implementation of the CommandExecutor. The framework will do in some parts like the command executor.
    /// </remarks>
    public static IServiceScope CreateScope()
    {
        Ensure.Boolean.IsTrue(IsInitialized)
            .ElseThrowsInvalidOperation("Cannot create a scope if the container is not initialized.");

        var scope = _serviceProvider!.CreateScope();
        SetScopeIntoLocator(scope);

        return scope;
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

        SetProviderIntoLocator();
        SetScopeIntoLocator(null);
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