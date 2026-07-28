using System;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Hosting;

namespace IOKode.OpinionatedFramework.ContractImplementations.Hangfire;

public sealed class HangfireWorker : IHostedService, IDisposable
{
    private readonly Lock sync = new();
    private readonly BackgroundJobServerOptions serverOptions;
    private BackgroundJobServer? server;

    /// <summary>
    /// Creates a worker that starts a background job server with the supplied options.
    /// </summary>
    /// <param name="serverOptions">
    /// The server options built during bootstrap from the root <c>Hangfire</c> configuration section and the
    /// <c>Hangfire</c> bootstrap verb.
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="serverOptions"/> is <see langword="null"/>.</exception>
    public HangfireWorker(BackgroundJobServerOptions serverOptions)
    {
        ArgumentNullException.ThrowIfNull(serverOptions);

        this.serverOptions = serverOptions;
    }

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (this.sync)
        {
            if (this.server is not null)
            {
                throw new InvalidOperationException("The Hangfire worker has already been started.");
            }

            this.server = new BackgroundJobServer(this.serverOptions);
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
