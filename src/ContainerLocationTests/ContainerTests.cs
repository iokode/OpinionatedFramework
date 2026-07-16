using System;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.ServiceContainer;
using IOKode.OpinionatedFramework.ServiceLocation;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IOKode.OpinionatedFramework.ContainerLocationTests;

public interface ITestService
{
    string GetMessage();
}

public class TestService : ITestService
{
    public string GetMessage() => "Hello, World!";
}

public sealed class AsyncDisposableScopedService : IAsyncDisposable
{
    public bool IsDisposed { get; private set; }

    public ValueTask DisposeAsync()
    {
        IsDisposed = true;
        return ValueTask.CompletedTask;
    }
}

public class ContainerTests : IDisposable
{
    public void Dispose()
    {
        Container.Advanced.ResetAsync().AsTask().GetAwaiter().GetResult();
    }
    
    [Fact]
    public void CanRegisterAndResolveService()
    {
        // Arrange
        Container.Services.AddSingleton<ITestService, TestService>();
        Container.Initialize();

        // Assert
        var testService = Locator.Resolve<ITestService>();
        Assert.NotNull(testService);
        Assert.IsType<TestService>(testService);
    }

    [Fact]
    public void CanResolveRegisteredSingletonService()
    {
        // Arrange
        Container.Services.AddSingleton<ITestService, TestService>();
        Container.Initialize();

        // Act
        var testService1 = Locator.Resolve<ITestService>();
        var testService2 = Locator.Resolve<ITestService>();

        // Assert
        Assert.NotNull(testService1);
        Assert.NotNull(testService2);
        Assert.Same(testService1, testService2);
    }

    [Fact]
    public void CanResolveRegisteredTransientService()
    {
        // Arrange
        Container.Services.AddTransient<ITestService, TestService>();
        Container.Initialize();

        // Act
        var testService1 = Locator.Resolve<ITestService>();
        var testService2 = Locator.Resolve<ITestService>();

        // Assert
        Assert.NotNull(testService1);
        Assert.NotNull(testService2);
        Assert.NotSame(testService1, testService2);
    }

    [Fact]
    public void ThrowsExceptionWhenResolvingUnregisteredService()
    {
        // Arrange
        Container.Initialize();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            Locator.Resolve<ITestService>();
        });
        Assert.Equal("No service of type 'IOKode.OpinionatedFramework.ContainerLocationTests.ITestService' has been registered.", exception.Message);
    }

    [Fact]
    public void ThrowsExceptionWhenResolvingOnNotInitializedContainer()
    {
        // Arrange
        Container.Services.AddTransient<ITestService, TestService>();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            Locator.Resolve<ITestService>();
        });
        Assert.Equal("The container is not initialized. Call Container.Initialize().", exception.Message);
    }

    [Fact]
    public void ThrowsExceptionWhenRegisteringServicesOnInitializedContainer()
    {
        // Arrange
        Container.Initialize();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            Container.Services.AddTransient<ITestService, TestService>();
        });
        Assert.Equal("The service collection cannot be modified because it is read-only.", exception.Message);
    }

    [Fact]
    public void ThrowsExceptionWhenTryingToInitializeContainerMultipleTimes()
    {
        // Arrange
        Container.Initialize();
        
        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            Container.Initialize();
        });
        Assert.Equal("The container has already been initialized. It can only be initialized once.", exception.Message);
    }

    [Fact]
    public async Task ScopeHandleDisposesAndUnregistersScope()
    {
        Container.Services.AddScoped<AsyncDisposableScopedService>();
        Container.Initialize();

        var scope = Container.Advanced.CreateScope();
        var service = Locator.Resolve<AsyncDisposableScopedService>();
        Assert.Same(service, scope.ServiceProvider.GetRequiredService<AsyncDisposableScopedService>());

        await scope.DisposeAsync();

        Assert.True(service.IsDisposed);
        Assert.Throws<ObjectDisposedException>(() => scope.ServiceProvider);
    }

    [Fact]
    public void NestedScopesAreRejected()
    {
        Container.Initialize();
        _ = Container.Advanced.CreateScope();

        var exception = Assert.Throws<InvalidOperationException>(() => Container.Advanced.CreateScope());

        Assert.Equal("Nested service scopes are not supported.", exception.Message);
    }

    [Fact]
    public async Task ScopeCanBeDisposedByIdentifier()
    {
        Container.Services.AddScoped<AsyncDisposableScopedService>();
        Container.Initialize();
        var scope = Container.Advanced.CreateScope();
        var service = Locator.Resolve<AsyncDisposableScopedService>();

        await Container.Advanced.DisposeScopeAsync(scope);

        Assert.True(service.IsDisposed);
        await scope.DisposeAsync();
    }

    [Fact]
    public async Task NewScopeReplacesStaleScopeIdDisposedFromAnotherExecutionContext()
    {
        Container.Initialize();
        var disposedScope = Container.Advanced.CreateScope();

        await Task.Run(async () => await Container.Advanced.DisposeScopeAsync(disposedScope));
        await using var replacementScope = Container.Advanced.CreateScope();

        Assert.NotEqual(disposedScope.Id, replacementScope.Id);
    }

    [Fact]
    public async Task ContainerDisposalDisposesScopesFromAllExecutionContexts()
    {
        Container.Services.AddScoped<AsyncDisposableScopedService>();
        Container.Initialize();
        var firstScope = await Task.Run(() =>
        {
            var scope = Container.Advanced.CreateScope();
            return (Scope: scope, Service: Locator.Resolve<AsyncDisposableScopedService>());
        });
        var secondScope = Container.Advanced.CreateScope();
        var secondService = Locator.Resolve<AsyncDisposableScopedService>();

        await Container.Advanced.DisposeAsync();

        Assert.True(firstScope.Service.IsDisposed);
        Assert.True(secondService.IsDisposed);
        await firstScope.Scope.DisposeAsync();
        await secondScope.DisposeAsync();
    }
}
