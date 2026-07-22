using System;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.HostingIntegrations;
using IOKode.OpinionatedFramework.ServiceContainer;
using IOKode.OpinionatedFramework.ServiceLocation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace IOKode.OpinionatedFramework.Tests.HostingIntegrations;

public class OpinionatedFrameworkServiceProviderFactoryTests : IAsyncLifetime
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
    public void HostDisposalDisposesContainerAndRootProviderOnce()
    {
        Container.Services.AddSingleton<CountingAsyncDisposable>();
        var host = Host.CreateDefaultBuilder()
            .UseServiceProviderFactory(new OpinionatedFrameworkServiceProviderFactory())
            .Build();
        var disposableService = host.Services.GetRequiredService<CountingAsyncDisposable>();

        host.Dispose();

        Assert.True(Container.IsDisposed);
        Assert.Null(Locator.ServiceProvider);
        Assert.Equal(1, disposableService.DisposeCount);
    }

    private sealed class CountingAsyncDisposable : IAsyncDisposable
    {
        public int DisposeCount { get; private set; }

        public ValueTask DisposeAsync()
        {
            this.DisposeCount++;
            return ValueTask.CompletedTask;
        }
    }
}
