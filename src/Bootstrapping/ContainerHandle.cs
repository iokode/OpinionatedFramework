using System;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.ServiceContainer;

namespace IOKode.OpinionatedFramework.Bootstrapping;

/// <summary>Owns the initialized framework container for a non-hosted application.</summary>
internal sealed class ContainerHandle : IAsyncDisposable
{
    private bool isDisposed;

    /// <summary>Asynchronously disposes the framework container and its instantiated services.</summary>
    public async ValueTask DisposeAsync()
    {
        if (this.isDisposed)
        {
            return;
        }

        this.isDisposed = true;
        await Container.Advanced.DisposeAsync();
    }
}
