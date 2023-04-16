#pragma warning disable CS0649

using System;

namespace IOKode.OpinionatedFramework.Foundation;

/// <summary>
/// Provides a static service locator for resolving services registered in the Container class.
/// </summary>
/// <remarks>
/// The Locator class is designed to provide an easy way to access services registered
/// in the Container class. It uses a static service provider to resolve services based on their types.
/// Before using the Locator, make sure to initialize it by calling the Container.Initialize() method.
/// </remarks>
public static class Locator
{
    private static IServiceProvider? _serviceProvider;

    /// <summary>
    /// Gets the current instance of the service provider used by the locator.
    /// </summary>
    /// <value>
    /// The service provider instance or null if it has not been initialized.
    /// </value>
    /// <remarks>
    /// The service provider is set by the Container class during the initialization process.
    /// It is used to resolve services registered in the Container. Do not modify the service
    /// provider directly; instead, use the Container class to manage services.
    /// </remarks>
    public static IServiceProvider? ServiceProvider => _serviceProvider;

    /// <summary>
    /// Resolve a service based on type.
    /// </summary>
    /// <param name="serviceType">The type.</param>
    /// <returns>The resolved service.</returns>
    /// <exception cref="InvalidOperationException">Thrown when trying to resolver a non-registered service or the container is not initialized.</exception>
    public static object Resolve(Type serviceType)
    {
        if (_serviceProvider == null)
        {
            throw new InvalidOperationException("The container is not initialized. Call Container.Initialize().");
        }

        var service = _serviceProvider.GetService(serviceType);

        if (service == null)
        {
            throw new InvalidOperationException($"No service of type '{serviceType.FullName}' has been registered.");
        }

        return service;
    }

    /// <summary>
    /// Resolve a service based on type.
    /// </summary>
    /// <typeparam name="TService">The type.</typeparam>
    /// <returns>The resolved service.</returns>
    /// <exception cref="InvalidOperationException">Thrown when trying to resolver a non-registered service or the container is not initialized.</exception>
    public static TService Resolve<TService>()
    {
        var service = (TService)Resolve(typeof(TService));
        return service;
    }
}