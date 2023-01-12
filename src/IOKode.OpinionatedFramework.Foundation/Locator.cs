using System;
using System.Collections.Generic;

namespace IOKode.OpinionatedFramework.Foundation;

public static class Locator
{
    // ReSharper disable once CollectionNeverUpdated.Local
    private static readonly Dictionary<Type, Func<object>> _resolverFunctions = new();

    public static object Resolve(Type serviceType)
    {
        var resolverFunction = _getResolverFunctionForType(serviceType);
        var service = resolverFunction();
        return service;
    }

    public static TService Resolve<TService>()
    {
        var service = (TService)Resolve(typeof(TService));
        return service;
    }

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