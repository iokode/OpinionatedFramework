using System;
using System.Collections.Generic;
using System.Reflection;
using IOKode.OpinionatedFramework.Foundation;

namespace IOKode.OpinionatedFramework.ConfigureApplication;

public static class LocatorConfiguration
{
    public static void Register<TService>(Func<TService> resolverFunction)
    {
        var field = typeof(Locator).GetField("_resolverFunctions",
            BindingFlags.Static | BindingFlags.NonPublic);

        var resolverFunctionsDict = (Dictionary<Type, Func<object>>)field!.GetValue(null)!;
        object objectResolverFunction() => resolverFunction()!;
        resolverFunctionsDict.Add(typeof(TService), objectResolverFunction);
    }
}