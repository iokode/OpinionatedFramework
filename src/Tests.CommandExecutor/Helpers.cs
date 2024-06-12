using System;
using System.Collections.Generic;
using IOKode.OpinionatedFramework.Bootstrapping;
using IOKode.OpinionatedFramework.Commands;
using IOKode.OpinionatedFramework.ContractImplementations.MicrosoftLogging;
using Microsoft.Extensions.DependencyInjection;

namespace IOKode.OpinionatedFramework.Tests.CommandExecutor;

public static class Helpers
{
    public static object _lockObj = new();

    public static ICommandExecutor CreateExecutor(CommandMiddleware[]? middlewares = null,
        IEnumerable<KeyValuePair<string, object>>? sharedData = default)
    {
        Container.Advanced.Clear();
        Container.Services.AddMicrosoftLogging(_ => { });
        Container.Initialize();
        var executor =
            new ContractImplementations.CommandExecutor.CommandExecutor(
                middlewares ?? Array.Empty<CommandMiddleware>(),
                sharedData);
        return executor;
    }

    public static ICommandExecutor CreateExecutorWithSampleScopedService(CommandMiddleware[]? middlewares = null,
        IEnumerable<KeyValuePair<string, object>>? sharedData = default)
    {
        return CreateExecutor(() => Container.Services.AddScoped<SampleService>(), middlewares, sharedData);
    }

    public static ICommandExecutor CreateExecutor(Action configureContainer, CommandMiddleware[]? middlewares = null,
        IEnumerable<KeyValuePair<string, object>>? sharedData = default)
    {
        Container.Advanced.Clear();
        Container.Services.AddMicrosoftLogging(_ => { });
        configureContainer();
        Container.Initialize();

        var executor =
            new ContractImplementations.CommandExecutor.CommandExecutor(middlewares ?? Array.Empty<CommandMiddleware>(),
                sharedData);
        return executor;
    }
}