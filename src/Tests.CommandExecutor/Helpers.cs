using System;
using IOKode.OpinionatedFramework.ServiceContainer;
using IOKode.OpinionatedFramework.ServiceLocation;
using IOKode.OpinionatedFramework.Commands;
using IOKode.OpinionatedFramework.ContractImplementations.CommandExecutor;
using IOKode.OpinionatedFramework.ContractImplementations.MicrosoftEALogging;
using Microsoft.Extensions.DependencyInjection;

namespace IOKode.OpinionatedFramework.Tests.CommandExecutor;

internal static class Helpers
{
    public static ICommandExecutor CreateExecutor(Action<CommandExecutorOptions> optionsAction)
    {
        Container.Advanced.ResetAsync().AsTask().GetAwaiter().GetResult();
        Container.Services.AddLogging();
        Container.Services.AddMicrosoftExtensionsAbstractionsLogging();
        Container.Initialize();

        var executor = new ContractImplementations.CommandExecutor.CommandExecutor(optionsAction);
        return executor;
    }

    public static ICommandExecutor CreateExecutorWithSampleScopedService(Action<CommandExecutorOptions> optionsAction)
    {
        return CreateExecutor(() => Container.Services.AddScoped<SampleService>(), optionsAction);
    }

    /// <summary>
    /// Creates an executor and registers it in the container, so that commands invoked from inside another command
    /// through the InvokeAsync extension resolve it.
    /// </summary>
    public static ICommandExecutor CreateRegisteredExecutorWithSampleScopedService(Action<CommandExecutorOptions> optionsAction)
    {
        Container.Advanced.ResetAsync().AsTask().GetAwaiter().GetResult();
        Container.Services.AddLogging();
        Container.Services.AddMicrosoftExtensionsAbstractionsLogging();
        Container.Services.AddScoped<SampleService>();
        Container.Services.AddSingleton<ICommandExecutor>(
            _ => new ContractImplementations.CommandExecutor.CommandExecutor(optionsAction));
        Container.Initialize();

        return Locator.Resolve<ICommandExecutor>();
    }

    public static ICommandExecutor CreateExecutor(Action configureContainer, Action<CommandExecutorOptions> optionsAction)
    {
        Container.Advanced.ResetAsync().AsTask().GetAwaiter().GetResult();
        Container.Services.AddLogging();
        Container.Services.AddMicrosoftExtensionsAbstractionsLogging();
        configureContainer();
        Container.Initialize();

        var executor = new ContractImplementations.CommandExecutor.CommandExecutor(optionsAction);
        return executor;
    }
}
