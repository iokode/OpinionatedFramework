using System;
using IOKode.OpinionatedFramework.Ensuring;
using Microsoft.Extensions.DependencyInjection;

namespace IOKode.OpinionatedFramework.Bootstrapping;

public static partial class Container
{
    /// <summary>
    /// Advanced actions over the container. Typically, you shouldn't call these methods directly, except in advanced
    /// scenarios. The framework will do in some parts.
    /// </summary>
    public static class Advanced
    {
        /// <summary>
        /// Creates a new service scope that can be used to resolve scoped services and sets it in the Locator.
        /// </summary>
        /// <returns>A new IServiceScope that can be used to resolve scoped services.</returns>
        /// <exception cref="InvalidOperationException">Thrown when trying to create a scope before the container is initialized.</exception>
        /// <remarks>
        /// Scoped services are disposed when the scope is disposed. When the scope was used, it should be disposed
        /// calling <see cref="DisposeScope"/> method.
        /// </remarks>
        public static IServiceScope CreateScope()
        {
            Ensure.Boolean.IsTrue(IsInitialized)
                .ElseThrowsInvalidOperation("Cannot create a scope if the container is not initialized.");

            var scope = _serviceProvider!.CreateScope();
            SetScopeIntoLocator(scope);
            _serviceProviderScope = scope;

            return scope;
        }

        /// <summary>
        /// Disposes and removes the service scope.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the container isn't initialized.</exception>
        public static void DisposeScope()
        {
            Ensure.Boolean.IsTrue(IsInitialized)
                .ElseThrowsInvalidOperation("Cannot dispose the scope because the container is not initialized.");

            _serviceProviderScope?.Dispose();
            _serviceProviderScope = null;
            SetScopeIntoLocator(null);
        }

        /// <summary>
        /// Clears the current service collection and resets the service provider, effectively allowing the container to be reconfigured.
        /// </summary>
        /// <remarks>
        /// This method should be used with caution, as it will discard all previously registered services and set the service provider to null.
        /// After calling this method, you should re-register your services and call Initialize() again.
        /// This method is primarily intended for use in testing scenarios where the container's state needs to be reset between test runs.
        /// </remarks>
        public static void Clear()
        {
            if (IsInitialized)
            {
                DisposeScope();
            }

            _serviceCollection = new OpinionatedServiceCollection();
            _serviceProvider = null;

            SetProviderIntoLocator();
            SetScopeIntoLocator(null);
        }

        /// <summary>
        /// Clears the current service collection and then, register the sames services that was registered. It allow
        /// to change descriptors at runtime.
        /// </summary>
        /// <remarks>
        /// This method should be used with caution, as it will set the service provider to null.
        /// After calling this method, you should re-register your services and call Initialize() again.
        /// This method is intended for use in advanced scenarios where is required to modify descriptors at runtime.
        /// Instead of using this method, the recommended way to replace descriptors is restart the application and
        /// let the initial configuration to set the new descriptors.
        /// </remarks>
        public static void ClearWithRegisteredServices()
        {
            var collection = OpinionatedServiceCollection.Copy(_serviceCollection);
            Clear();
            _serviceCollection = collection;
        }
    }
}