using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IOKode.OpinionatedFramework.Bootstrapping;

/// <summary>Owns the host and initialized framework container.</summary>
public sealed class HostHandle : IAsyncDisposable
{
    private readonly IHost host;
    private readonly IHostApplicationLifetime applicationLifetime;
    private readonly Task hostLifetimeTask;
    private int explicitDisposalRequested;

    internal HostHandle(IHost host)
    {
        this.host = host;
        this.applicationLifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
        this.hostLifetimeTask = this.RunHostLifetimeAsync();
    }

    /// <summary>Gets the token that is canceled when application shutdown is requested.</summary>
    public CancellationToken ApplicationStopping => this.applicationLifetime.ApplicationStopping;

    /// <summary>Requests application shutdown.</summary>
    public void StopApplication()
    {
        this.applicationLifetime.StopApplication();
    }

    /// <summary>Stops the host and disposes the framework container.</summary>
    public async ValueTask DisposeAsync()
    {
        Interlocked.Exchange(ref this.explicitDisposalRequested, 1);
        this.applicationLifetime.StopApplication();
        await this.hostLifetimeTask;
    }

    private async Task RunHostLifetimeAsync()
    {
        try
        {
            try
            {
                await this.host.WaitForShutdownAsync();
            }
            finally
            {
                this.host.Dispose();
            }
        }
        finally
        {
            if (Volatile.Read(ref this.explicitDisposalRequested) == 0)
            {
                Environment.Exit(Environment.ExitCode);
            }
        }
    }
}
