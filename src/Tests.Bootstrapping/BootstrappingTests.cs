using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Bootstrapping;
using IOKode.OpinionatedFramework.Commands;
using IOKode.OpinionatedFramework.Emailing;
using IOKode.OpinionatedFramework.Jobs;
using IOKode.OpinionatedFramework.ServiceContainer;
using IOKode.OpinionatedFramework.ServiceLocation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace IOKode.OpinionatedFramework.Tests.Bootstrapping;

public class BootstrappingTests : IAsyncLifetime
{
    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await Container.Advanced.ResetAsync();
    }

    [Fact]
    public async Task BootstrapRegistersSafeDefaults()
    {
        var configuration = new ConfigurationBuilder().Build();

        await using var runtime = await OpinionatedFrameworkBootstrapping.StartAsync(configuration);

        Assert.NotNull(Locator.Resolve<ICommandExecutor>());
        Assert.NotNull(Locator.Resolve<IEmailSender>());
        Assert.NotNull(Locator.Resolve<IJobEnqueuer>());
        Assert.NotNull(Locator.Resolve<IJobScheduler>());
    }

    [Fact]
    public async Task HostedServicesExecuteInStartupAndReverseShutdownOrder()
    {
        var configuration = new ConfigurationBuilder().Build();
        var operations = new List<string>();
        Container.Services.AddSingleton<IHostedService>(_ =>
            new RecordingHostedService("start-first", "stop-first", operations));
        Container.Services.AddSingleton<IHostedService>(_ =>
            new RecordingHostedService("start-second", "stop-second", operations));

        var hostHandle = await OpinionatedFrameworkBootstrapping.StartAsync(configuration);
        await hostHandle.DisposeAsync();

        Assert.Equal(["start-first", "start-second", "stop-second", "stop-first"], operations);
    }

    [Fact]
    public async Task StartupFailureDisposesInstantiatedContainerServices()
    {
        var configuration = new ConfigurationBuilder().Build();
        var operations = new List<string>();
        Container.Services.AddSingleton<IHostedService>(_ =>
            new DisposableRecordingHostedService("start-first", "dispose-first", operations));
        Container.Services.AddSingleton<IHostedService>(new FailingHostedService(operations));
        Container.Services.AddSingleton<IHostedService>(_ =>
            new DisposableRecordingHostedService("must-not-start", "dispose-not-started", operations));

        await Assert.ThrowsAsync<TestStartupException>(() =>
            OpinionatedFrameworkBootstrapping.StartAsync(configuration));

        Assert.Equal(
            ["start-first", "start-failing", "dispose-not-started", "dispose-first"],
            operations);
    }

    private sealed class RecordingHostedService(
        string startupOperation,
        string shutdownOperation,
        ICollection<string> operations) : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken = default)
        {
            operations.Add(startupOperation);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            operations.Add(shutdownOperation);
            return Task.CompletedTask;
        }
    }

    private sealed class DisposableRecordingHostedService(
        string startupOperation,
        string disposalOperation,
        ICollection<string> operations) : IHostedService, IAsyncDisposable
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            operations.Add(startupOperation);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public ValueTask DisposeAsync()
        {
            operations.Add(disposalOperation);
            return ValueTask.CompletedTask;
        }
    }

    private sealed class FailingHostedService(ICollection<string> operations) : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken = default)
        {
            operations.Add("start-failing");
            throw new TestStartupException();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class TestStartupException : Exception;
}
