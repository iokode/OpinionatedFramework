using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Bootstrapping.Abstractions;
using IOKode.OpinionatedFramework.Commands;
using IOKode.OpinionatedFramework.Emailing;
using IOKode.OpinionatedFramework.Jobs;
using IOKode.OpinionatedFramework.ServiceContainer;
using IOKode.OpinionatedFramework.ServiceLocation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IOKode.OpinionatedFramework.Tests.Bootstrapping;

public class BootstrappingTests : IAsyncLifetime
{
    static BootstrappingTests()
    {
        BootstrapDriverCatalog.Register<FirstInvalidDriverRegistrar>(
            typeof(IFirstInvalidDriver), "FirstInvalidDriver", "invalid", false, false, "Tests.Bootstrapping");
        BootstrapDriverCatalog.Register<SecondInvalidDriverRegistrar>(
            typeof(ISecondInvalidDriver), "SecondInvalidDriver", "invalid", false, false, "Tests.Bootstrapping");
        BootstrapDriverCatalog.Register<NamedDriverRegistrar>(
            typeof(INamedDriver), "NamedDrivers", "shared", false, true, "Tests.Bootstrapping");
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await Container.Advanced.ResetAsync();
        NamedDriverRegistrar.Reset();
    }

    [Fact]
    public void GeneratedCatalogContainsReferencedPackageDrivers()
    {
        Assert.Contains(BootstrapDriverCatalog.RegisteredDrivers,
            driver => driver.ContractType == typeof(IEmailSender) && driver.DriverKey == "logger");
        Assert.Contains(BootstrapDriverCatalog.RegisteredDrivers,
            driver => driver.ContractType == typeof(IEmailSender) && driver.DriverKey == "mailkit");
        Assert.Contains(BootstrapDriverCatalog.RegisteredDrivers,
            driver => driver.ContractType == typeof(IJobEnqueuer) && driver.DriverKey == "task-run");
        Assert.Contains(BootstrapDriverCatalog.RegisteredDrivers,
            driver => driver.ContractType == typeof(IJobScheduler) && driver.DriverKey == "task-run");
    }

    [Fact]
    public async Task BootstrapRegistersSafeDefaults()
    {
        var configuration = new ConfigurationBuilder().Build();

        await using var runtime =
            await IOKode.OpinionatedFramework.Bootstrapping.OpinionatedFrameworkBootstrapping.StartAsync(configuration);

        Assert.NotNull(Locator.Resolve<ICommandExecutor>());
        Assert.NotNull(Locator.Resolve<IEmailSender>());
        Assert.NotNull(Locator.Resolve<IJobEnqueuer>());
        Assert.NotNull(Locator.Resolve<IJobScheduler>());
    }

    [Fact]
    public void SelectedDriverIsValidatedBeforeRegistration()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OpinionatedFramework:Email:Driver"] = "mailkit"
            })
            .Build();

        var exception = Assert.Throws<BootstrapConfigurationException>(() =>
            IOKode.OpinionatedFramework.Bootstrapping.OpinionatedFrameworkBootstrapping.RegisterDrivers(configuration));

        Assert.Contains("Email:Host", exception.Message);
        Assert.Contains("Email:Port", exception.Message);
        Assert.Equal(2, exception.Errors.Count);
    }

    [Fact]
    public void UnknownDriverReportsAvailableKeys()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OpinionatedFramework:Email:Driver"] = "missing"
            })
            .Build();

        var exception = Assert.Throws<BootstrapConfigurationException>(() =>
            IOKode.OpinionatedFramework.Bootstrapping.OpinionatedFrameworkBootstrapping.RegisterDrivers(configuration));

        Assert.Contains("Available drivers: logger, mailkit", exception.Message);
    }

    [Fact]
    public void ValidationErrorsFromMultipleDriversAreAggregated()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OpinionatedFramework:FirstInvalidDriver:Driver"] = "invalid",
                ["OpinionatedFramework:SecondInvalidDriver:Driver"] = "invalid"
            })
            .Build();

        var exception = Assert.Throws<BootstrapConfigurationException>(() =>
            IOKode.OpinionatedFramework.Bootstrapping.OpinionatedFrameworkBootstrapping.RegisterDrivers(configuration));

        Assert.Collection(exception.Errors,
            error => Assert.Equal("OpinionatedFramework:FirstInvalidDriver:Value", error.ConfigurationPath),
            error => Assert.Equal("OpinionatedFramework:SecondInvalidDriver:Value", error.ConfigurationPath));
    }

    [Fact]
    public void NamedDriversShareStateWithinOneBootstrapCall()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OpinionatedFramework:NamedDrivers:first:Driver"] = "shared",
                ["OpinionatedFramework:NamedDrivers:second:Driver"] = "shared"
            })
            .Build();

        IOKode.OpinionatedFramework.Bootstrapping.OpinionatedFrameworkBootstrapping.RegisterDrivers(configuration);

        Assert.Equal(1, NamedDriverRegistrar.StateCreations);
        Assert.Equal(2, NamedDriverRegistrar.RegisteredInstances);
    }

    [Fact]
    public async Task LifecycleTasksExecuteInStartupAndReverseShutdownOrder()
    {
        IOKode.OpinionatedFramework.Bootstrapping.OpinionatedFrameworkBootstrapping.RegisterDrivers(
            new ConfigurationBuilder().Build());
        var operations = new List<string>();
        Container.Services.AddSingleton<IStartupTask>(_ =>
            new RecordingLifecycleTask("start-first", "dispose-first", operations));
        Container.Services.AddSingleton<IStartupTask>(_ =>
            new RecordingLifecycleTask("start-second", "dispose-second", operations));

        IOKode.OpinionatedFramework.Bootstrapping.OpinionatedFrameworkBootstrapping.InitializeContainer();
        var lifecycleHandle = await IOKode.OpinionatedFramework.Bootstrapping.OpinionatedFrameworkBootstrapping.StartLifecycleAsync();
        await lifecycleHandle.DisposeAsync();

        Assert.Equal(["start-first", "start-second", "dispose-second", "dispose-first"], operations);
    }

    [Fact]
    public async Task StartupFailureDisposesInstantiatedContainerServices()
    {
        IOKode.OpinionatedFramework.Bootstrapping.OpinionatedFrameworkBootstrapping.RegisterDrivers(
            new ConfigurationBuilder().Build());
        var operations = new List<string>();
        Container.Services.AddSingleton<IStartupTask>(_ =>
            new RecordingLifecycleTask("start-first", "dispose-first", operations));
        Container.Services.AddSingleton<IStartupTask>(new FailingStartupTask(operations));
        Container.Services.AddSingleton<IStartupTask>(_ =>
            new RecordingLifecycleTask("must-not-start", "dispose-not-started", operations));

        IOKode.OpinionatedFramework.Bootstrapping.OpinionatedFrameworkBootstrapping.InitializeContainer();
        await Assert.ThrowsAsync<TestStartupException>(() =>
            IOKode.OpinionatedFramework.Bootstrapping.OpinionatedFrameworkBootstrapping.StartLifecycleAsync());

        Assert.Equal(
            ["start-first", "start-failing", "dispose-not-started", "dispose-first"],
            operations);
    }

    private interface IFirstInvalidDriver;
    private interface ISecondInvalidDriver;
    private interface INamedDriver;

    private sealed class FirstInvalidDriverRegistrar : IBootstrapDriverRegistrar
    {
        public static BootstrapValidationResult Validate(BootstrapDriverContext context)
        {
            return BootstrapValidationResult.Failure(new BootstrapValidationError(
                $"{context.DriverConfiguration.Path}:Value", "First invalid value."));
        }

        public static void Register(BootstrapDriverContext context)
        {
            throw new Xunit.Sdk.XunitException("An invalid driver must not be registered.");
        }
    }

    private sealed class SecondInvalidDriverRegistrar : IBootstrapDriverRegistrar
    {
        public static BootstrapValidationResult Validate(BootstrapDriverContext context)
        {
            return BootstrapValidationResult.Failure(new BootstrapValidationError(
                $"{context.DriverConfiguration.Path}:Value", "Second invalid value."));
        }

        public static void Register(BootstrapDriverContext context)
        {
            throw new Xunit.Sdk.XunitException("An invalid driver must not be registered.");
        }
    }

    private sealed class NamedDriverRegistrar : IBootstrapDriverRegistrar
    {
        private sealed class SharedRegistrationState
        {
            public int Instances { get; set; }
        }

        public static int StateCreations { get; private set; }
        public static int RegisteredInstances { get; private set; }

        public static BootstrapValidationResult Validate(BootstrapDriverContext context)
        {
            return BootstrapValidationResult.Success;
        }

        public static void Register(BootstrapDriverContext context)
        {
            var state = context.GetOrAddSharedState("Tests.NamedDriver", () =>
            {
                StateCreations++;
                return new SharedRegistrationState();
            });
            state.Instances++;
            RegisteredInstances = state.Instances;
        }

        public static void Reset()
        {
            StateCreations = 0;
            RegisteredInstances = 0;
        }
    }

    private sealed class RecordingLifecycleTask(
        string startupOperation,
        string disposalOperation,
        ICollection<string> operations) : IStartupTask, IAsyncDisposable
    {
        public Task StartAsync(CancellationToken cancellationToken = default)
        {
            operations.Add(startupOperation);
            return Task.CompletedTask;
        }

        public ValueTask DisposeAsync()
        {
            operations.Add(disposalOperation);
            return ValueTask.CompletedTask;
        }
    }

    private sealed class FailingStartupTask(ICollection<string> operations) : IStartupTask
    {
        public Task StartAsync(CancellationToken cancellationToken = default)
        {
            operations.Add("start-failing");
            throw new TestStartupException();
        }
    }

    private sealed class TestStartupException : Exception;
}
