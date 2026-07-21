using System;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Hosting;

namespace IOKode.OpinionatedFramework.ContractImplementations.Hangfire;

public sealed class HangfireWorker : IHostedService, IDisposable
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

    public Task StopAsync(CancellationToken cancellationToken)
    {
        this.Dispose();
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
