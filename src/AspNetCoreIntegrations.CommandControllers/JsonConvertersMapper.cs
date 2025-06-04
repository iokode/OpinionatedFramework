using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;

namespace IOKode.OpinionatedFramework.AspNetCoreIntegrations.CommandControllers;

internal static class JsonConvertersMapper
{
    private static readonly List<Type> converters = [];
    public static readonly IReadOnlyCollection<Type> Converters = converters;

    public static void RegisterConverter(Type converterType)
    {
        if (!converterType.IsAssignableTo(typeof(JsonConverter)) && !converterType.IsAssignableToOpenGeneric(typeof(JsonConverter<>)))
        {
            throw new ArgumentException("Converter type must be a JsonConverter or a JsonConverter<T>.", nameof(converterType));
        }

        converters.Add(converterType);
    }

    public static void RegisterCommandsFromAssembly(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        var converterTypes = assembly.GetTypes()
            .Where(type => type is {IsClass: true, IsAbstract: false})
            .Where(type => type.IsAssignableTo(typeof(JsonConverter)) || type.IsAssignableToOpenGeneric(typeof(JsonConverter<>)));

        foreach (var converterType in converterTypes)
        {
            RegisterConverter(converterType);
        }
    }

    public static void RegisterCommandsFromAssemblies(ICollection<Assembly> assemblies)
    {
        foreach (var assembly in assemblies)
        {
            RegisterCommandsFromAssembly(assembly);
        }
    }
}