using System;
using IOKode.OpinionatedFramework.ConfigureApplication;
using IOKode.OpinionatedFramework.Foundation;
using Microsoft.Extensions.DependencyInjection;

namespace IOKode.OpinionatedFramework.Tests.Foundation;

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
        Assert.Throws<InvalidOperationException>(Locator.Resolve<ITestService>);
    }

    public void Dispose()
    {
        Container.Clear();
    }
}