using System;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using IOKode.OpinionatedFramework.Bootstrapping.Abstractions;

namespace IOKode.OpinionatedFramework.ContractImplementations.Hangfire.Jobs;

public sealed class HangfireWorker : IStartupTask, IDisposable
{
    private readonly Lock sync = new();
    private BackgroundJobServer? server;

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (this.sync)
        {
            if (this.server is not null)
            {
                throw new InvalidOperationException("The Hangfire worker has already been started.");
            }

            this.server = new BackgroundJobServer();
        }

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        BackgroundJobServer? serverToStop;
        lock (this.sync)
        {
            serverToStop = this.server;
            this.server = null;
        }

        if (serverToStop is null)
        {
            return;
        }

        serverToStop.SendStop();
        serverToStop.Dispose();
    }
}
