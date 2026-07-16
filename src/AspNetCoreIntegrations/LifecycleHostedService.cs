using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Bootstrapping.Abstractions;
using IOKode.OpinionatedFramework.ServiceLocation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IOKode.OpinionatedFramework.AspNetCoreIntegrations;

internal sealed class LifecycleHostedService : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var startupTasks = Locator.ServiceProvider!.GetServices<IStartupTask>();
        foreach (var startupTask in startupTasks)
        {
            await startupTask.StartAsync(cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
