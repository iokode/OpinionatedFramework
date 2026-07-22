using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace IOKode.OpinionatedFramework.ServiceLocation;

/// <summary>
/// Provides access to the root provider and the scope associated with the current asynchronous execution context.
/// </summary>
public static class Locator
{
    private static readonly ConcurrentDictionary<Guid, AsyncServiceScope> scopes = new();
    private static readonly AsyncLocal<Guid?> currentScopeId = new();
    private static IServiceProvider? rootServiceProvider;

    /// <summary>Gets the provider associated with the current scope, or the root provider when no scope is active.</summary>
    /// <exception cref="ObjectDisposedException">The current execution context references a disposed scope.</exception>
    public static IServiceProvider? ServiceProvider
    {
        get
        {
            if (!currentScopeId.Value.HasValue)
            {
                return rootServiceProvider;
            }

            var scopeId = currentScopeId.Value.Value;
            if (scopes.TryGetValue(scopeId, out var scope))
            {
                return scope.ServiceProvider;
            }

            throw new ObjectDisposedException($"Service scope '{scopeId}'");
        }
    }

    internal static IServiceProvider? RootServiceProvider => rootServiceProvider;

    internal static void SetRootServiceProvider(IServiceProvider? serviceProvider)
    {
        rootServiceProvider = serviceProvider;
    }

    internal static IServiceProvider? RemoveRootServiceProvider()
    {
        var serviceProvider = rootServiceProvider;
        rootServiceProvider = null;
        return serviceProvider;
    }

    internal static ScopeHandle CreateScope()
    {
        if (currentScopeId.Value.HasValue)
        {
            if (scopes.ContainsKey(currentScopeId.Value.Value))
            {
                throw new InvalidOperationException("Nested service scopes are not supported.");
            }

            currentScopeId.Value = null;
        }

        if (rootServiceProvider is null)
        {
            throw new InvalidOperationException("The root service provider is not initialized.");
        }

        var scopeId = Guid.NewGuid();
        var scope = rootServiceProvider.CreateAsyncScope();
        if (!scopes.TryAdd(scopeId, scope))
        {
            scope.Dispose();
            throw new InvalidOperationException("The service scope could not be registered.");
        }

        currentScopeId.Value = scopeId;
        return new ScopeHandle(scopeId);
    }

    internal static IServiceProvider GetScopeServiceProvider(Guid scopeId)
    {
        if (!scopes.TryGetValue(scopeId, out var scope))
        {
            throw new ObjectDisposedException($"Service scope '{scopeId}'");
        }

        return scope.ServiceProvider;
    }

    internal static ValueTask DisposeScopeAsync(Guid scopeId, bool throwIfNotFound)
    {
        if (!scopes.TryRemove(scopeId, out var scope))
        {
            if (currentScopeId.Value == scopeId)
            {
                currentScopeId.Value = null;
            }

            if (throwIfNotFound)
            {
                throw new ArgumentException($"Service scope '{scopeId}' does not exist.", nameof(scopeId));
            }

            return ValueTask.CompletedTask;
        }

        if (currentScopeId.Value == scopeId)
        {
            currentScopeId.Value = null;
        }

        return scope.DisposeAsync();
    }

    internal static async ValueTask DisposeScopesAsync()
    {
        var exceptions = new List<Exception>();
        foreach (var scopeId in scopes.Keys.ToArray())
        {
            if (!scopes.TryRemove(scopeId, out var scope))
            {
                continue;
            }

            try
            {
                await scope.DisposeAsync();
            }
            catch (Exception exception)
            {
                exceptions.Add(exception);
            }
        }

        currentScopeId.Value = null;
        if (exceptions.Count > 0)
        {
            throw new AggregateException("One or more service scopes could not be disposed.", exceptions);
        }
    }

    /// <summary>Resolves a service from the current scoped provider or root provider.</summary>
    public static object Resolve(Type serviceType)
    {
        if (ServiceProvider is null)
        {
            throw new InvalidOperationException("The container is not initialized. Call Container.Initialize().");
        }

        var service = ServiceProvider.GetService(serviceType);
        if (service is null)
        {
            throw new InvalidOperationException($"No service of type '{serviceType.FullName}' has been registered.");
        }

        return service;
    }

    /// <summary>Resolves a service from the current scoped provider or root provider.</summary>
    public static TService Resolve<TService>()
    {
        return (TService) Resolve(typeof(TService));
    }
}
