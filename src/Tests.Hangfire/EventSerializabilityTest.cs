using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Bootstrapping;
using IOKode.OpinionatedFramework.ContractImplementations.Hangfire;
using IOKode.OpinionatedFramework.Drivers.Abstractions;
using IOKode.OpinionatedFramework.ServiceContainer;
using IOKode.OpinionatedFramework.ServiceContainer.Drivers;
using IOKode.OpinionatedFramework.Tests.Hangfire.Config;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace IOKode.OpinionatedFramework.Tests.Hangfire;

/// <summary>
/// Covers the rule that this driver only accepts events it can store and read back.
/// </summary>
/// <remarks>
/// No fixture and no infrastructure: validation runs before anything is registered, so the application never
/// reaches the storage it was configured with.
/// </remarks>
public class EventSerializabilityTest : IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        await Container.Advanced.ResetAsync();
    }

    public async Task DisposeAsync()
    {
        await Container.Advanced.ResetAsync();
    }

    [Fact]
    public async Task Bootstrap_validation_reports_an_event_that_cannot_be_read_back()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OpinionatedFramework:Events:Driver"] = "Hangfire"
            })
            .Build();

        var exception = await Assert.ThrowsAsync<BootstrapConfigurationException>(() =>
            OpinionatedFrameworkBootstrapping.StartAsync(configuration, options =>
                options.HangfireEvents(events => events.AddEvent<NotReadableEvent>())));

        // The driver rebuilds an event from what it stored, so an event it could not rebuild is a
        // configuration error and not a job that fails once it is already waiting.
        Assert.Contains("tests.not-readable", exception.Message, StringComparison.Ordinal);
        Assert.Contains("cannot be carried by this driver", exception.Message, StringComparison.Ordinal);
    }
}
