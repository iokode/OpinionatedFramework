using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IOKode.OpinionatedFramework.Commands;

namespace IOKode.OpinionatedFramework.AspNetCoreIntegrations.ControllerGenerators;

internal static class CommandsMapper
{
    private static readonly Dictionary<string, Type> commands = new();
    public static IReadOnlyDictionary<string, Type> Commands => commands;

    public static void RegisterCommand(string commandName, Type commandType)
    {
        ArgumentNullException.ThrowIfNull(commandType);
        commands[commandName] = commandType;
    }

    public static void RegisterCommandsFromAssembly(Assembly assembly, Func<Type, string>? commandNameProvider = null)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        var commandTypes = assembly.GetTypes()
            .Where(type => type is {IsClass: true, IsAbstract: false})
            .Where(type => type.IsAssignableTo(typeof(Command)) || type.IsAssignableToOpenGeneric(typeof(Command<>)));

        foreach (var commandType in commandTypes)
        {
            var commandName = commandNameProvider?.Invoke(commandType) ?? commandType.FullName;
            RegisterCommand(commandName!, commandType);
        }
    }

    public static void RegisterCommandsFromAssemblies(ICollection<Assembly> assemblies, Func<Type, string>? commandNameProvider = null)
    {
        foreach (var assembly in assemblies)
        {
            RegisterCommandsFromAssembly(assembly, commandNameProvider);
        }
    }
}