using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Commands;
using IOKode.OpinionatedFramework.ConfigureApplication;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IOKode.OpinionatedFramework.Tests.CommandExecutor;

public partial class CommandExecutorTests
{
    [Fact]
    public async Task InvokeCommandMaintainsScopeInDeep_Success()
    {
        // Arrange
        Container.Clear();
        Container.Services.AddScoped<SampleService>();
        Container.Services.AddTransient<ICommandExecutor, ContractImplementations.CommandExecutor.CommandExecutor>(_ => new ContractImplementations.CommandExecutor.CommandExecutor(Array.Empty<ICommandMiddleware>()));
        Container.Initialize();

        // Act
        var command = new InDeepCommand();
        var servicesFrom1stExecution = await command.InvokeAsync();
        var servicesFrom2ndExecution = await command.InvokeAsync();

        // Assert
        Assert.Same(servicesFrom1stExecution.Item1, servicesFrom1stExecution.Item2);
        Assert.Same(servicesFrom2ndExecution.Item1, servicesFrom2ndExecution.Item2);
        Assert.NotSame(servicesFrom1stExecution.Item1, servicesFrom2ndExecution.Item1);
    }
    
    [Fact]
    public async Task InvokesCommandWithScopedService_SameServiceIsResolved()
    {
        // Arrange
        Container.Clear();
        Container.Services.AddScoped<SampleService>();
        Container.Services.AddTransient<ICommandExecutor, ContractImplementations.CommandExecutor.CommandExecutor>(_ => new ContractImplementations.CommandExecutor.CommandExecutor(Array.Empty<ICommandMiddleware>()));
        Container.Initialize();

        // Act
        var command = new SampleCommand();
        var servicesFrom1stExecution = await command.InvokeAsync();
        var servicesFrom2ndExecution = await command.InvokeAsync();

        // Assert
        Assert.Same(servicesFrom1stExecution.Item1, servicesFrom1stExecution.Item2);
        Assert.Same(servicesFrom2ndExecution.Item1, servicesFrom2ndExecution.Item2);
        Assert.NotSame(servicesFrom1stExecution.Item1, servicesFrom2ndExecution.Item1);
    }

    [Fact]
    public async Task InvokeCommandAsync_DoesNotOverrideServiceScopes()
    {
        // Arrange
        Container.Clear();
        Container.Services.AddScoped<ScopedStateService>();
        Container.Services.AddTransient<ICommandExecutor, ContractImplementations.CommandExecutor.CommandExecutor>(_ => new ContractImplementations.CommandExecutor.CommandExecutor(Array.Empty<ICommandMiddleware>()));
        Container.Initialize();

        var command = new SampleCommandWithScopedState();
        var tasks = new List<Task<string>>();

        // Act
        for (int i = 0; i < 100; i++)
        {
            // Run 100 commands concurrently
            tasks.Add(Task.Run(async () => await command.InvokeAsync()));
        }

        var results = await Task.WhenAll(tasks);

        // Assert
        // Check that each command got its own unique scoped state
        Assert.Equal(100, results.Length); // 100 commands
        Assert.Equal(100, results.Distinct().Count()); // Each scoped state should be unique
    }

    [Fact]
    public async Task InvokeCommandAsync_EnsureScopedServiceProviderDoesNotRemains()
    {
        // Arrange
        Container.Clear();
        Container.Services.AddTransient<ICommandExecutor, ContractImplementations.CommandExecutor.CommandExecutor>(_ => new ContractImplementations.CommandExecutor.CommandExecutor(Array.Empty<ICommandMiddleware>()));
        Container.Services.AddScoped<SampleService>();
        Container.Initialize();

        // Act
        var command = new GetProviderCommand();
        var provider = await command.InvokeAsync();

        // Arrange
        Assert.NotSame(Container.Services, provider);
        var asyncLocalSp = (AsyncLocal<IServiceProvider?>)typeof(Locator).GetField("_scopedServiceProvider",
            BindingFlags.Static | BindingFlags.NonPublic)!.GetValue(null)!;
        Assert.Null(asyncLocalSp.Value);
    }
    
    public class SampleService
    {
    }

    private class SampleCommand : Command<(SampleService, SampleService)>
    {
        protected override Task<(SampleService, SampleService)> ExecuteAsync(CommandContext context)
        {
            var service = Locator.Resolve<SampleService>();
            var service2 = Locator.Resolve<SampleService>();

            return Task.FromResult((service, service2));
        }
    }

    private class GetProviderCommand : Command<IServiceProvider>
    {
        protected override Task<IServiceProvider> ExecuteAsync(CommandContext context)
        {
            return Task.FromResult(Locator.ServiceProvider!);
        }
    }

    private class ScopedStateService
    {
        public string State { get; set; }

        public ScopedStateService()
        {
            State = Guid.NewGuid().ToString();
        }
    }

    private class SampleCommandWithScopedState : Command<string>
    {
        protected override async Task<string> ExecuteAsync(CommandContext context)
        {
            var scopedStateService = Locator.Resolve<ScopedStateService>();
            var scopedStateToModify = Locator.Resolve<ScopedStateService>();

            scopedStateToModify.State += "-modified";

            int delay = new Random().Next(1000, 4000);
            await Task.Delay(delay);

            return scopedStateService.State;
        }
    }

    private class InDeepCommand : Command<(SampleService, SampleService)>
    {
        public class InnerSyncClass
        {
            public SampleService Method()
            {
                return Locator.Resolve<SampleService>();
            }
        }

        public class InnerAsyncClass
        {
            public async Task<SampleService> Method()
            {
                var service = Locator.Resolve<SampleService>();
                await Task.Delay(1000);
                return service;
            }
        }

        protected override async Task<(SampleService, SampleService)> ExecuteAsync(CommandContext context)
        {
            var sync = new InnerSyncClass();
            var async = new InnerAsyncClass();
            var item1 = sync.Method();
            var item2 = await async.Method();

            return (item1, item2);
        }
    }
}