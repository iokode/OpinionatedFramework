using System.Collections.Generic;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Drivers.Abstractions;
using IOKode.OpinionatedFramework.Emailing;
using IOKode.OpinionatedFramework.Jobs;
using IOKode.OpinionatedFramework.ServiceContainer;
using IOKode.OpinionatedFramework.ServiceContainer.Drivers;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace IOKode.OpinionatedFramework.Tests.ServiceContainer.Drivers;

public class DriverRegistrationTests : IAsyncLifetime
{
    static DriverRegistrationTests()
    {
        BootstrapDriverCatalog.Register<FirstInvalidDriverRegistrar>(
            typeof(IFirstInvalidDriver), "FirstInvalidDriver", "invalid", false, false,
            "Tests.ServiceContainer.Drivers");
        BootstrapDriverCatalog.Register<SecondInvalidDriverRegistrar>(
            typeof(ISecondInvalidDriver), "SecondInvalidDriver", "invalid", false, false,
            "Tests.ServiceContainer.Drivers");
        BootstrapDriverCatalog.Register<NamedDriverRegistrar>(
            typeof(INamedDriver), "NamedDrivers", "shared", false, true,
            "Tests.ServiceContainer.Drivers");
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
    public void SelectedDriverIsValidatedBeforeRegistration()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OpinionatedFramework:Email:Driver"] = "mailkit"
            })
            .Build();

        var exception = Assert.Throws<BootstrapConfigurationException>(() =>
            DriverRegistration.RegisterDrivers(configuration));

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
            DriverRegistration.RegisterDrivers(configuration));

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
            DriverRegistration.RegisterDrivers(configuration));

        Assert.Collection(exception.Errors,
            error => Assert.Equal("OpinionatedFramework:FirstInvalidDriver:Value", error.ConfigurationPath),
            error => Assert.Equal("OpinionatedFramework:SecondInvalidDriver:Value", error.ConfigurationPath));
    }

    [Fact]
    public void NamedDriversShareStateWithinOneRegistrationCall()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OpinionatedFramework:NamedDrivers:first:Driver"] = "shared",
                ["OpinionatedFramework:NamedDrivers:second:Driver"] = "shared"
            })
            .Build();

        DriverRegistration.RegisterDrivers(configuration);

        Assert.Equal(1, NamedDriverRegistrar.StateCreations);
        Assert.Equal(2, NamedDriverRegistrar.RegisteredInstances);
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
}
