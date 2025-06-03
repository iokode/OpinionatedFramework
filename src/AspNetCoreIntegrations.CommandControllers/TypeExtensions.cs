using System;
using System.Linq;

namespace IOKode.OpinionatedFramework.AspNetCoreIntegrations.CommandControllers;

internal static class TypeExtensions
{
    public static bool IsAssignableToOpenGeneric(this Type type, Type openGenericType)
    {
        if (type == typeof(object))
        {
            return false;
        }

        for (var currentType = type; currentType != null; currentType = currentType.BaseType)
        {
            Type[] types = [currentType, ..currentType.GetInterfaces()];
            if (types.Any(candidateType => candidateType.IsGenericType && candidateType.GetGenericTypeDefinition() == openGenericType))
            {
                return true;
            }
        }

        return false;
    }
}