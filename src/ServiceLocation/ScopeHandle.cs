using System;
using System.Threading;
using System.Threading.Tasks;

namespace IOKode.OpinionatedFramework.ServiceLocation;

/// <summary>Owns one service scope registered with the locator.</summary>
public sealed class ScopeHandle : IAsyncDisposable
{
    private int disposed;

    internal ScopeHandle(Guid id)
    {
        Id = id;
    }

    /// <summary>Gets the identifier assigned to the scope.</summary>
    public Guid Id { get; }

    /// <summary>Gets the service provider owned by the scope.</summary>
    /// <exception cref="ObjectDisposedException">The scope has been disposed.</exception>
    public IServiceProvider ServiceProvider => Locator.GetScopeServiceProvider(Id);

    /// <summary>Removes and asynchronously disposes the scope.</summary>
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref disposed, 1) != 0)
        {
            return;
        }

        await Locator.DisposeScopeAsync(Id, false);
    }
}
