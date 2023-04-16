using System;
using System.Collections.Generic;

namespace IOKode.OpinionatedFramework.Foundation;

public static class Locator
{
#pragma warning disable CS0649
    private static IServiceProvider? _serviceProvider;
#pragma warning restore CS0649

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