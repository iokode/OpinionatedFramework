using System;
using IOKode.OpinionatedFramework.ConfigureApplication;
using Microsoft.Extensions.DependencyInjection;

namespace IOKode.OpinionatedFramework.Tests.Foundation.Foundation;

public interface ITestService
{
    string GetMessage();
}

public class TestService : ITestService
{
    public string GetMessage() => "Hello, World!";
}

public class ContainerTests : IDisposable
{
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
        Assert.Equal("No service of type 'IOKode.OpinionatedFramework.Tests.Foundation.Foundation.ITestService' has been registered.", exception.Message);
    }

    [Fact]
    public void ThrowsExceptionWhenContainerIsNotInitialized()
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

    public void Dispose()
    {
        Container.Clear();
    }
}