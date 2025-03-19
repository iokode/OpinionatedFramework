using System;
using System.Collections.Generic;
using IOKode.OpinionatedFramework.Bootstrapping;
using IOKode.OpinionatedFramework.Commands;
using IOKode.OpinionatedFramework.ContractImplementations.CommandExecutor;
using IOKode.OpinionatedFramework.ContractImplementations.MicrosoftLogging;
using Microsoft.Extensions.DependencyInjection;

namespace IOKode.OpinionatedFramework.Tests.CommandExecutor;

internal static class Helpers
{
    public static ICommandExecutor CreateExecutor(Action<CommandExecutorOptions> optionsAction)
    {
        Container.Advanced.Clear();
        Container.Services.AddMicrosoftLogging(_ => { });
        Container.Initialize();

        var executor = new ContractImplementations.CommandExecutor.CommandExecutor(optionsAction);
        return executor;
    }

    public static ICommandExecutor CreateExecutorWithSampleScopedService(Action<CommandExecutorOptions> optionsAction)
    {
        return CreateExecutor(() => Container.Services.AddScoped<SampleService>(), optionsAction);
    }

    public static ICommandExecutor CreateExecutor(Action configureContainer, Action<CommandExecutorOptions> optionsAction)
    {
        Container.Advanced.Clear();
        Container.Services.AddMicrosoftLogging(_ => { });
        configureContainer();
        Container.Initialize();

        var executor = new ContractImplementations.CommandExecutor.CommandExecutor(optionsAction);
        return executor;
    }
}