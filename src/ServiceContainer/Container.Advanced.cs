using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.ServiceLocation;

namespace IOKode.OpinionatedFramework.ServiceContainer;

public static partial class Container
{
    /// <summary>Provides advanced container lifecycle and scope operations.</summary>
    public static class Advanced
    {
        /// <summary>
        /// Participates in the service scope active in the current asynchronous execution context, or creates and
        /// selects one when there is none.
        /// </summary>
        /// <remarks>
        /// This is what any code that takes part in an ongoing operation should use, so that a command invoked from
        /// another command, or a query invoked from a command, resolves the very same scoped services as its caller.
        /// Scopes are never nested: when a scope is already active, the returned handle points to it and disposing
        /// that handle does nothing, leaving the scope to be disposed by whoever created it. Code that begins an
        /// operation of its own, such as a job execution, must use <see cref="CreateIndependentScope"/> instead.
        /// </remarks>
        /// <returns>
        /// A handle that asynchronously disposes the scope when it created one, and does nothing when it participates
        /// in an already active scope.
        /// </returns>
        /// <exception cref="InvalidOperationException">The container is disposed or uninitialized.</exception>
        public static ScopeHandle CreateScope()
        {
            EnsureInitializedAndNotDisposed();

            return Locator.CreateScope();
        }

        /// <summary>
        /// Creates and selects a service scope for the current asynchronous execution context, regardless of whether
        /// a scope is already active there.
        /// </summary>
        /// <remarks>
        /// Intended for code that begins an operation with a lifetime of its own, such as executing a job or a retry
        /// attempt of one. Such code must not participate in the scope of whoever triggered it: the asynchronous
        /// execution context flows into background tasks, so the scope in effect at that moment may well be disposed
        /// while the operation is still running. The new scope resolves its scoped services independently, and the
        /// previously active scope, if any, is selected again once the returned handle is disposed.
        /// </remarks>
        /// <returns>A caller-owned handle that asynchronously disposes the scope.</returns>
        /// <exception cref="InvalidOperationException">The container is disposed or uninitialized.</exception>
        public static ScopeHandle CreateIndependentScope()
        {
            EnsureInitializedAndNotDisposed();

            return Locator.CreateIndependentScope();
        }

        /// <summary>Removes and asynchronously disposes the identified service scope.</summary>
        /// <param name="handle">A handle owning its scope, as returned by <see cref="CreateScope"/> or <see cref="CreateIndependentScope"/>.</param>
        /// <exception cref="ArgumentException">The identifier does not represent an active scope.</exception>
        /// <exception cref="InvalidOperationException">
        /// The container is disposed or uninitialized, or the handle participates in a scope owned by an outer handle.
        /// </exception>
        public static ValueTask DisposeScopeAsync(ScopeHandle handle)
        {
            EnsureInitializedAndNotDisposed();

            return handle.DisposeAsync(true);
        }

        /// <summary>Disposes every registered scope, the root provider, and instantiated disposable services.</summary>
        public static async ValueTask DisposeAsync()
        {
            EnsureNotDisposed();

            _isDisposed = true;
            var serviceProvider = Locator.RemoveRootServiceProvider();
            var exceptions = new List<Exception>();
            try
            {
                await Locator.DisposeScopesAsync();
            }
            catch (Exception exception)
            {
                exceptions.Add(exception);
            }

            if (serviceProvider is not null)
            {
                try
                {
                    await ((IAsyncDisposable) serviceProvider).DisposeAsync();
                }
                catch (Exception exception)
                {
                    exceptions.Add(exception);
                }
            }

            if (exceptions.Count > 0)
            {
                throw new AggregateException("The container could not be completely disposed.", exceptions);
            }
        }

        /// <summary>Disposes the container and replaces its service collection.</summary>
        public static async ValueTask ResetAsync()
        {
            if (!IsDisposed)
            {
                await DisposeAsync();
            }

            _serviceCollection = new OpinionatedServiceCollection();
            _isDisposed = false;
        }

        /// <summary>Resets the container while retaining a copy of its service descriptors.</summary>
        public static async ValueTask ResetWithRegisteredServicesAsync()
        {
            var collection = OpinionatedServiceCollection.Copy(_serviceCollection);
            await ResetAsync();
            _serviceCollection = collection;
        }
    }
}
