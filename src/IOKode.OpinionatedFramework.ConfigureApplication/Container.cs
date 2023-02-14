using System;
using System.Collections.Generic;
using System.Reflection;
using IOKode.OpinionatedFramework.Foundation;

namespace IOKode.OpinionatedFramework.ConfigureApplication;

public static class Container
{
    public static void Register<TService>(Func<TService> resolverFunction)
    {
        var resolverFunctionsDict = _getResolverFunctionsDictionary();
        object objectResolverFunction() => resolverFunction()!;
        resolverFunctionsDict.Add(typeof(TService), objectResolverFunction);
    }

    private static Dictionary<Type, Func<object>>? _resolverFunctionsDictionary;
    private static Dictionary<Type, Func<object>> _getResolverFunctionsDictionary()
    {
        if (_resolverFunctionsDictionary != null)
        {
            return _resolverFunctionsDictionary;
        }

        var field = typeof(Locator).GetField("_resolverFunctions",
            BindingFlags.Static | BindingFlags.NonPublic);

        _resolverFunctionsDictionary = (Dictionary<Type, Func<object>>)field!.GetValue(null)!;
        return _resolverFunctionsDictionary;
    }
}