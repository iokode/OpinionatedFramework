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
        /// <summary>Creates, registers, and selects a service scope for the current asynchronous execution context.</summary>
        /// <returns>A caller-owned handle that asynchronously disposes the scope.</returns>
        /// <exception cref="InvalidOperationException">
        /// The container is disposed or uninitialized, or the current execution context already has an active scope.
        /// </exception>
        public static ScopeHandle CreateScope()
        {
            EnsureInitializedAndNotDisposed();

            return Locator.CreateScope();
        }

        /// <summary>Removes and asynchronously disposes the identified service scope.</summary>
        /// <param name="handle">The handle returned by <see cref="CreateScope"/>.</param>
        /// <exception cref="ArgumentException">The identifier does not represent an active scope.</exception>
        /// <exception cref="InvalidOperationException">The container is disposed or uninitialized.</exception>
        public static ValueTask DisposeScopeAsync(ScopeHandle handle)
        {
            EnsureInitializedAndNotDisposed();

            return Locator.DisposeScopeAsync(handle.Id, true);
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
