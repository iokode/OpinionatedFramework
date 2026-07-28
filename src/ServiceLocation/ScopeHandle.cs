using System;
using System.Threading;
using System.Threading.Tasks;

namespace IOKode.OpinionatedFramework.ServiceLocation;

/// <summary>
/// Represents the use of one service scope by the code that requested it.
/// </summary>
/// <remarks>
/// A handle either owns the scope it points to or merely participates in a scope owned by an outer handle. Only the
/// owner disposes the scope, so participating handles can be disposed unconditionally, including through
/// <see langword="await using"/>.
/// </remarks>
public sealed class ScopeHandle : IAsyncDisposable
{
    private readonly Guid? restoredScopeId;
    private int disposed;

    private ScopeHandle(Guid id, bool ownsScope, Guid? restoredScopeId)
    {
        Id = id;
        OwnsScope = ownsScope;
        this.restoredScopeId = restoredScopeId;
    }

    /// <summary>Creates a handle that owns the scope and restores <paramref name="restoredScopeId"/> once disposed.</summary>
    internal static ScopeHandle Owned(Guid id, Guid? restoredScopeId)
    {
        return new ScopeHandle(id, true, restoredScopeId);
    }

    /// <summary>Creates a handle that participates in a scope owned by an outer handle.</summary>
    internal static ScopeHandle Participating(Guid id)
    {
        return new ScopeHandle(id, false, null);
    }

    /// <summary>Gets the identifier assigned to the scope.</summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets a value indicating whether this handle owns the scope. A handle that participates in an already active
    /// scope does not own it and never disposes it.
    /// </summary>
    public bool OwnsScope { get; }

    /// <summary>Gets the service provider owned by the scope.</summary>
    /// <exception cref="ObjectDisposedException">The scope has been disposed.</exception>
    public IServiceProvider ServiceProvider => Locator.GetScopeServiceProvider(Id);

    /// <summary>
    /// Removes and asynchronously disposes the scope when this handle owns it; otherwise does nothing.
    /// </summary>
    public ValueTask DisposeAsync()
    {
        return DisposeAsync(false);
    }

    internal ValueTask DisposeAsync(bool throwIfNotDisposable)
    {
        if (!OwnsScope)
        {
            if (throwIfNotDisposable)
            {
                throw new InvalidOperationException(
                    "The scope is owned by an outer handle and cannot be disposed through this one.");
            }

            return ValueTask.CompletedTask;
        }

        if (Interlocked.Exchange(ref disposed, 1) != 0)
        {
            if (throwIfNotDisposable)
            {
                throw new ArgumentException($"Service scope '{Id}' does not exist.", nameof(ScopeHandle));
            }

            return ValueTask.CompletedTask;
        }

        return Locator.DisposeScopeAsync(Id, restoredScopeId, throwIfNotDisposable);
    }
}
