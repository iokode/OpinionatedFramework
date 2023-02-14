using System;
using System.Collections.Generic;

namespace IOKode.OpinionatedFramework.Foundation;

public static class Locator
{
    // ReSharper disable once CollectionNeverUpdated.Local
    private static readonly Dictionary<Type, Func<object>> _resolverFunctions = new();

    /// <summary>
    /// Resolve a service based on type.
    /// </summary>
    /// <param name="serviceType">The type.</param>
    /// <returns>The resolved service.</returns>
    /// <exception cref="InvalidOperationException">Thrown when trying to resolver a non-registered service.</exception>
    public static object Resolve(Type serviceType)
    {
        var resolverFunction = _getResolverFunctionForType(serviceType);
        var service = resolverFunction();
        return service;
    }

    /// <summary>
    /// Resolve a service based on type.
    /// </summary>
    /// <typeparam name="TService">The type.</typeparam>
    /// <returns>The resolved service.</returns>
    /// <exception cref="InvalidOperationException">Thrown when trying to resolver a non-registered service.</exception>
    public static TService Resolve<TService>()
    {
        var service = (TService)Resolve(typeof(TService));
        return service;
    }

    /// <exception cref="InvalidOperationException">Thrown when trying to resolver a non-registered service.</exception>
    private static Func<object> _getResolverFunctionForType(Type type)
    {
        bool isResolved = _resolverFunctions.TryGetValue(type, out var resolverFunction);
        if (!isResolved)
        {
            throw new InvalidOperationException("Trying to resolve a non-registered service.");
        }

        return resolverFunction!;
    }
}