using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using IOKode.OpinionatedFramework.ContractImplementations.Hangfire;
using IOKode.OpinionatedFramework.Drivers.Abstractions;
using IOKode.OpinionatedFramework.ServiceContainer;
using IOKode.OpinionatedFramework.ServiceContainer.Drivers;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace IOKode.OpinionatedFramework.Tests.Bootstrapping;

/// <summary>
/// Covers the Hangfire driver configuration surface.
/// </summary>
/// <remarks>
/// The drivers are registered without starting a host, because starting the background job server would need a
/// real storage. The tests never configure one, so <c>JobStorage.Current</c> stays uninitialized for the test
/// that asserts a missing storage is reported.
/// </remarks>
public class HangfireBootstrapTests : IAsyncLifetime
{
    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await Container.Advanced.ResetAsync();
    }

    [Fact]
    public void ConfiguredServerSettingsReachTheWorker()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["Hangfire:Queues:0"] = "events",
            ["Hangfire:Queues:1"] = "reports",
            ["Hangfire:WorkerCount"] = "7",
            ["Hangfire:ShutdownTimeout"] = "00:00:42"
        });

        DriverRegistration.RegisterDrivers(configuration, BuildOptions());

        var serverOptions = GetRegisteredServerOptions();
        Assert.Equal(["events", "reports"], serverOptions.Queues);
        Assert.Equal(7, serverOptions.WorkerCount);
        Assert.Equal(TimeSpan.FromSeconds(42), serverOptions.ShutdownTimeout);
    }

    [Fact]
    public void CodeConfigurationOverridesConfiguredServerSettings()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["Hangfire:Queues:0"] = "events",
            ["Hangfire:WorkerCount"] = "4"
        });

        DriverRegistration.RegisterDrivers(configuration, BuildOptions(hangfire =>
            hangfire.ConfigureServer(server => server.WorkerCount = 9)));

        var serverOptions = GetRegisteredServerOptions();
        Assert.Equal(9, serverOptions.WorkerCount);

        // A setting the delegate does not touch keeps the configured value.
        Assert.Equal(["events"], serverOptions.Queues);
    }

    [Fact]
    public void AbsentSettingsKeepTheHangfireDefaults()
    {
        var defaults = new BackgroundJobServerOptions();

        DriverRegistration.RegisterDrivers(BuildConfiguration(), BuildOptions());

        var serverOptions = GetRegisteredServerOptions();
        Assert.Equal(defaults.Queues, serverOptions.Queues);
        Assert.Equal(defaults.WorkerCount, serverOptions.WorkerCount);
        Assert.Equal(defaults.ShutdownTimeout, serverOptions.ShutdownTimeout);
    }

    [Fact]
    public void ServerConfigurationReachesTheWorkerThroughEitherContract()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            // The scheduler alone selects the driver, so the enqueuer falls back to its default.
            ["OpinionatedFramework:JobEnqueuer:Driver"] = "task-run"
        });

        DriverRegistration.RegisterDrivers(configuration, BuildOptions(hangfire =>
            hangfire.ConfigureServer(server => server.WorkerCount = 3)));

        Assert.Equal(3, GetRegisteredServerOptions().WorkerCount);
    }

    [Fact]
    public void DisabledWorkerRegistersNoHostedService()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["Hangfire:StartWorker"] = "false"
        });

        DriverRegistration.RegisterDrivers(configuration, BuildOptions());

        Assert.DoesNotContain(Container.Services, service => service.ServiceType == typeof(HangfireWorker));
    }

    [Fact]
    public void WorkerIsRegisteredOnlyOnceForBothContracts()
    {
        DriverRegistration.RegisterDrivers(BuildConfiguration(), BuildOptions());

        Assert.Single(Container.Services, service => service.ServiceType == typeof(HangfireWorker));
    }

    [Fact]
    public void MissingStorageIsReported()
    {
        var configuration = BuildConfiguration();

        var errors = AssertValidationFails(configuration, new BootstrapOptions());

        var error = Assert.Single(errors);
        Assert.Equal("Hangfire", error.ConfigurationPath);
    }

    [Theory]
    [InlineData("StartWorker", "yes")]
    [InlineData("WorkerCount", "many")]
    [InlineData("WorkerCount", "0")]
    [InlineData("WorkerCount", "-1")]
    [InlineData("ShutdownTimeout", "forever")]
    [InlineData("ShutdownTimeout", "-00:00:30")]
    public void InvalidSettingIsReported(string key, string value)
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            [$"Hangfire:{key}"] = value
        });

        var errors = AssertValidationFails(configuration, BuildOptions());

        var error = Assert.Single(errors);
        Assert.Equal($"Hangfire:{key}", error.ConfigurationPath);
    }

    [Fact]
    public void EmptyQueueNameIsReported()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["Hangfire:Queues:0"] = "events",
            ["Hangfire:Queues:1"] = " "
        });

        var errors = AssertValidationFails(configuration, BuildOptions());

        var error = Assert.Single(errors);
        Assert.Equal("Hangfire:Queues:1", error.ConfigurationPath);
    }

    [Fact]
    public void EveryInvalidSettingIsReportedOnce()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["Hangfire:StartWorker"] = "yes",
            ["Hangfire:WorkerCount"] = "many"
        });

        var errors = AssertValidationFails(configuration, BuildOptions());

        // Both contracts select the same driver, so the shared errors are reported once rather than twice.
        Assert.Equal(
            ["Hangfire:StartWorker", "Hangfire:WorkerCount"],
            errors.Select(error => error.ConfigurationPath).Order());
    }

    [Fact]
    public void HangfireOptionsFailBootstrapWhenTheDriverIsNotSelected()
    {
        var configuration = new ConfigurationBuilder().Build();
        var options = new BootstrapOptions();
        options.Hangfire(hangfire => hangfire.ConfigureHangfire(_ => { }));

        var exception = Assert.Throws<BootstrapConfigurationException>(() =>
            DriverRegistration.RegisterDrivers(configuration, options));

        Assert.Contains(nameof(HangfireOptions), exception.Message);
    }

    private static IConfiguration BuildConfiguration(Dictionary<string, string?>? settings = null)
    {
        var values = new Dictionary<string, string?>(settings ?? [])
        {
            ["OpinionatedFramework:JobScheduler:Driver"] = "hangfire"
        };
        values.TryAdd("OpinionatedFramework:JobEnqueuer:Driver", "hangfire");
        values.TryAdd("Hangfire:StartWorker", "true");

        return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
    }

    /// <summary>
    /// Builds options carrying a Hangfire configurator, which is what the storage validation looks for. The
    /// configurator does nothing, so no process-wide storage is set.
    /// </summary>
    private static BootstrapOptions BuildOptions(Action<HangfireOptions>? configure = null)
    {
        var options = new BootstrapOptions();
        options.Hangfire(hangfire =>
        {
            hangfire.ConfigureHangfire(_ => { });
            configure?.Invoke(hangfire);
        });

        return options;
    }

    private static BackgroundJobServerOptions GetRegisteredServerOptions()
    {
        var descriptor = Assert.Single(Container.Services,
            service => service.ServiceType == typeof(BackgroundJobServerOptions));
        return Assert.IsType<BackgroundJobServerOptions>(descriptor.ImplementationInstance);
    }

    private static IReadOnlyCollection<BootstrapValidationError> AssertValidationFails(IConfiguration configuration,
        BootstrapOptions options)
    {
        var exception = Assert.Throws<BootstrapConfigurationException>(() =>
            DriverRegistration.RegisterDrivers(configuration, options));
        return exception.Errors;
    }
}
