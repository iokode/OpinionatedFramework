using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Dashboard;
using IOKode.OpinionatedFramework.ContractImplementations.Hangfire;
using IOKode.OpinionatedFramework.Drivers.Abstractions;
using IOKode.OpinionatedFramework.ServiceContainer;
using IOKode.OpinionatedFramework.ServiceContainer.Drivers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace IOKode.OpinionatedFramework.Tests.Bootstrapping;

/// <summary>
/// Covers the Hangfire driver configuration surface.
/// </summary>
/// <remarks>
/// The tests assert on service registrations without building a provider, because resolving any Hangfire type
/// runs the configuration <c>AddHangfire</c> defers and would need a real storage. Keeping every storage
/// configurator a no-op also leaves <c>JobStorage.Current</c> uninitialized, which the missing-storage test
/// depends on. The settings actually reaching a running server are covered in Tests.Hangfire.
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
    public void BootstrapRegistersTheHangfireSurface()
    {
        DriverRegistration.RegisterDrivers(BuildConfiguration(), BuildOptions());

        // AddHangfire contributes the types an application resolves, notably the ones UseHangfireDashboard needs.
        Assert.Contains(Container.Services, service => service.ServiceType == typeof(IGlobalConfiguration));
        Assert.Contains(Container.Services, service => service.ServiceType == typeof(JobStorage));
        Assert.Contains(Container.Services, service => service.ServiceType == typeof(RouteCollection));
        Assert.Contains(Container.Services, service => service.ServiceType == typeof(IBackgroundJobClient));
        Assert.Contains(Container.Services, service => service.ServiceType == typeof(IRecurringJobManager));
    }

    [Fact]
    public void EveryConfiguredServerIsRegistered()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["Hangfire:Servers:default:Queues:0"] = "default",
            ["Hangfire:Servers:events:Queues:0"] = "events",
            ["Hangfire:Servers:events:WorkerCount"] = "2"
        });

        DriverRegistration.RegisterDrivers(configuration, BuildOptions());

        Assert.Equal(2, Container.Services.Count(IsHangfireServer));
    }

    [Fact]
    public void NoConfiguredServerStartsNoServer()
    {
        // A process that only enqueues is described by leaving Servers out entirely.
        DriverRegistration.RegisterDrivers(BuildConfiguration(withDefaultServer: false), BuildOptions());

        Assert.DoesNotContain(Container.Services, IsHangfireServer);

        // Hangfire itself is still registered, because the enqueuer and the scheduler need it with or without
        // a server in this process.
        Assert.Contains(Container.Services, service => service.ServiceType == typeof(JobStorage));
    }

    [Fact]
    public void TheServerIsRegisteredForAContractThatSelectsTheDriverAlone()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            // The scheduler alone selects the driver, so the enqueuer falls back to its default.
            ["OpinionatedFramework:JobEnqueuer:Driver"] = "TaskRun"
        });

        DriverRegistration.RegisterDrivers(configuration, BuildOptions());

        Assert.Single(Container.Services, IsHangfireServer);
    }

    [Fact]
    public void HangfireIsRegisteredOnlyOnceForBothContracts()
    {
        DriverRegistration.RegisterDrivers(BuildConfiguration(), BuildOptions());

        Assert.Single(Container.Services, service => service.ServiceType == typeof(JobStorage));
        Assert.Single(Container.Services, IsHangfireServer);
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
    [InlineData("WorkerCount", "many")]
    [InlineData("WorkerCount", "0")]
    [InlineData("WorkerCount", "-1")]
    [InlineData("ShutdownTimeout", "forever")]
    [InlineData("ShutdownTimeout", "-00:00:30")]
    public void InvalidSettingIsReported(string key, string value)
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            [$"Hangfire:Servers:events:{key}"] = value
        });

        var errors = AssertValidationFails(configuration, BuildOptions());

        // The path names the entry, so the offending server is identifiable.
        var error = Assert.Single(errors);
        Assert.Equal($"Hangfire:Servers:events:{key}", error.ConfigurationPath);
    }

    [Fact]
    public void EmptyQueueNameIsReported()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["Hangfire:Servers:events:Queues:0"] = "events",
            ["Hangfire:Servers:events:Queues:1"] = " "
        });

        var errors = AssertValidationFails(configuration, BuildOptions());

        var error = Assert.Single(errors);
        Assert.Equal("Hangfire:Servers:events:Queues:1", error.ConfigurationPath);
    }

    [Fact]
    public void EveryInvalidSettingIsReportedOnce()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["Hangfire:Servers:default:WorkerCount"] = "many",
            ["Hangfire:Servers:events:ShutdownTimeout"] = "forever"
        });

        var errors = AssertValidationFails(configuration, BuildOptions());

        // Both contracts select the same driver, so the shared errors are reported once rather than twice.
        Assert.Equal(
            ["Hangfire:Servers:default:WorkerCount", "Hangfire:Servers:events:ShutdownTimeout"],
            errors.Select(error => error.ConfigurationPath).Order());
    }

    [Fact]
    public void OptionsForAServerThatIsNotConfiguredAreReported()
    {
        var options = BuildOptions(hangfire =>
            hangfire.ConfigureServer("reports", server => server.WorkerCount = 2));

        var errors = AssertValidationFails(BuildConfiguration(), options);

        var error = Assert.Single(errors);
        Assert.Equal("Hangfire:Servers:reports", error.ConfigurationPath);
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

    private static IConfiguration BuildConfiguration(Dictionary<string, string?>? settings = null,
        bool withDefaultServer = true)
    {
        var values = new Dictionary<string, string?>(settings ?? [])
        {
            ["OpinionatedFramework:JobScheduler:Driver"] = "Hangfire"
        };
        values.TryAdd("OpinionatedFramework:JobEnqueuer:Driver", "Hangfire");
        if (withDefaultServer && !values.Keys.Any(key => key.StartsWith("Hangfire:Servers:", StringComparison.Ordinal)))
        {
            values.Add("Hangfire:Servers:default:Queues:0", "default");
        }

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

    /// <summary>
    /// Matches the hosted service <c>AddHangfireServer</c> registers, told apart from the one the
    /// <c>TaskRun</c> driver registers by the assembly its factory comes from.
    /// </summary>
    private static bool IsHangfireServer(ServiceDescriptor service)
    {
        return service.ServiceType == typeof(IHostedService) &&
               service.ImplementationFactory?.Method.DeclaringType?.Namespace?.StartsWith("Hangfire", StringComparison.Ordinal) == true;
    }

    private static IReadOnlyCollection<BootstrapValidationError> AssertValidationFails(IConfiguration configuration,
        BootstrapOptions options)
    {
        var exception = Assert.Throws<BootstrapConfigurationException>(() =>
            DriverRegistration.RegisterDrivers(configuration, options));
        return exception.Errors;
    }
}
