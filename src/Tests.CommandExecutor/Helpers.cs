using System;
using System.Collections.Generic;
using IOKode.OpinionatedFramework.Bootstrapping;
using IOKode.OpinionatedFramework.Commands;
using IOKode.OpinionatedFramework.ContractImplementations.MicrosoftLogging;
using Microsoft.Extensions.DependencyInjection;

namespace IOKode.OpinionatedFramework.Tests.CommandExecutor;

internal static class Helpers
{
    public static ICommandExecutor CreateExecutor(CommandMiddleware[]? middlewares = null,
        Dictionary<string, object?>? sharedData = default)
    {
        Container.Advanced.Clear();
        Container.Services.AddMicrosoftLogging(_ => { });
        Container.Initialize();

        var executor = new ContractImplementations.CommandExecutor.CommandExecutor(middlewares ?? [], sharedData);
        return executor;
    }

    public static ICommandExecutor CreateExecutorWithSampleScopedService(CommandMiddleware[]? middlewares = null,
        Dictionary<string, object?>? sharedData = default)
    {
        return CreateExecutor(() => Container.Services.AddScoped<SampleService>(), middlewares, sharedData);
    }

    public static ICommandExecutor CreateExecutor(Action configureContainer, CommandMiddleware[]? middlewares = null,
        Dictionary<string, object?>? sharedData = default)
    {
        Container.Advanced.Clear();
        Container.Services.AddMicrosoftLogging(_ => { });
        configureContainer();
        Container.Initialize();

        var executor = new ContractImplementations.CommandExecutor.CommandExecutor(middlewares ?? [], sharedData);
        return executor;
    }
}