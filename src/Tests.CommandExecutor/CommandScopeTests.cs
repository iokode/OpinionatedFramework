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

public class CommandScopeTests
{
    [Fact]
    public async Task InvokeCommandMaintainsScopeInDeep_Success()
    {
        // Arrange
        var executor = Helpers.CreateExecutorWithSampleScopedService();

        // Act
        var cmd = new InDeepCommand();
        var servicesFromFirstExecution = await executor.InvokeAsync<InDeepCommand, (SampleService, SampleService)>(cmd, default);
        var servicesFromSecondExecution = await executor.InvokeAsync<InDeepCommand, (SampleService, SampleService)>(cmd, default);
        
        // Assert
        Assert.Same(servicesFromFirstExecution.Item1, servicesFromFirstExecution.Item2);
        Assert.Same(servicesFromSecondExecution.Item1, servicesFromSecondExecution.Item2);
        Assert.NotSame(servicesFromFirstExecution.Item1, servicesFromSecondExecution.Item1);
    }
    
    [Fact]
    public async Task InvokesCommandWithScopedService_SameServiceIsResolved()
    {
        // Arrange
        var executor = Helpers.CreateExecutorWithSampleScopedService();

        // Act
        var cmd = new SampleCommand();
        var servicesFromFirstExecution = await executor.InvokeAsync<SampleCommand, (SampleService, SampleService)>(cmd, default);
        var servicesFromSecondExecution = await executor.InvokeAsync<SampleCommand, (SampleService, SampleService)>(cmd, default);

        // Assert
        Assert.Same(servicesFromFirstExecution.Item1, servicesFromFirstExecution.Item2);
        Assert.Same(servicesFromSecondExecution.Item1, servicesFromSecondExecution.Item2);
        Assert.NotSame(servicesFromFirstExecution.Item1, servicesFromSecondExecution.Item1);
    }

    [Fact]
    public async Task InvokeCommandAsync_DoesNotOverrideServiceScopes()
    {
        // Arrange
        var executor = Helpers.CreateExecutor(() =>
        {
            Container.Services.AddScoped<ScopedStateService>();
        });

        var cmd = new SampleCommandWithScopedState();
        var tasks = new List<Task<string>>();

        // Act
        for (int i = 0; i < 1000; i++)
        {
            // Run 1000 cmds concurrently
            tasks.Add(Task.Run(async () => await executor.InvokeAsync<SampleCommandWithScopedState, string>(cmd, default)));
        }

        var results = await Task.WhenAll(tasks);

        // Assert
        // Check that each cmd got its own unique scoped state
        Assert.Equal(1000, results.Length); // 100 cmds
        Assert.Equal(1000, results.Distinct().Count()); // Each scoped state should be unique
    }

    [Fact]
    public async Task InvokeCommandAsync_EnsureScopedServiceProviderDoesNotRemains()
    {
        // Arrange
        var executor = Helpers.CreateExecutorWithSampleScopedService();

        // Act
        var cmd = new GetProviderCommand();
        var provider = await executor.InvokeAsync<GetProviderCommand, IServiceProvider>(cmd, default);

        // Arrange
        Assert.NotSame(Container.Services, provider);
        var asyncLocalSp = (AsyncLocal<IServiceProvider?>)typeof(Locator).GetField("_scopedServiceProvider",
            BindingFlags.Static | BindingFlags.NonPublic)!.GetValue(null)!;
        Assert.Null(asyncLocalSp.Value);
    }

    [Fact]
    public async Task InvokeCommandWithMiddleware_ScopeIsShared()
    {
        // Arrange
        var executor = Helpers.CreateExecutorWithSampleScopedService(new CommandMiddleware[]
            { new SetSampleServiceInSharedDataMiddleware() });
        
        // Act & Assert (assertions inside command)
        var cmd = new AssertSharedDateServiceIsSameCommand();
        await executor.InvokeAsync(cmd, default);
    }
    
    private class SetSampleServiceInSharedDataMiddleware : CommandMiddleware
    {
        public override Task ExecuteAsync(CommandContext context, InvokeNextMiddlewareDelegate nextAsync)
        {
            context.SetInSharedData("Service", Locator.Resolve<SampleService>());
            return Task.CompletedTask;
        }
    }

    private class AssertSharedDateServiceIsSameCommand : Command
    {
        protected override Task ExecuteAsync(CommandContext context)
        {
            var serviceFromSharedData = context.GetFromSharedDataOrDefault("Service");
            var serviceFromLocator = Locator.Resolve<SampleService>();
            
            Assert.Same(serviceFromLocator, serviceFromSharedData);
            
            return Task.CompletedTask;
        }
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