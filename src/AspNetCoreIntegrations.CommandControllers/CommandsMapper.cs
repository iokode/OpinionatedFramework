using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IOKode.OpinionatedFramework.Commands;

namespace IOKode.OpinionatedFramework.AspNetCoreIntegrations.CommandControllers;

internal static class CommandsMapper
{
    private static readonly Dictionary<string, Type> commands = new();
    public static IReadOnlyDictionary<string, Type> Commands => commands;

    public static void RegisterCommand(string commandName, Type commandType)
    {
        ArgumentNullException.ThrowIfNull(commandType);
        commands[commandName] = commandType;
    }

    public static void RegisterCommandsFromAssembly(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        var commandTypes = assembly.GetTypes()
            .Where(type => type is {IsClass: true, IsAbstract: false})
            .Where(type => type.IsAssignableTo(typeof(Command)) || type.IsAssignableToOpenGeneric(typeof(Command<>)));

        foreach (var commandType in commandTypes)
        {
            var commandName = commandType.Name;
            RegisterCommand(commandName, commandType);
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