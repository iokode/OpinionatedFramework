using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Bootstrapping;
using IOKode.OpinionatedFramework.ContractImplementations.MyServiceBus;
using IOKode.OpinionatedFramework.Drivers.Abstractions;
using IOKode.OpinionatedFramework.ServiceContainer;
using IOKode.OpinionatedFramework.ServiceContainer.Drivers;
using IOKode.OpinionatedFramework.Tests.MyServiceBus.RabbitMq.Config;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace IOKode.OpinionatedFramework.Tests.MyServiceBus.RabbitMq;

/// <summary>
/// Covers the rule that this driver only accepts events it can put on the wire and read back.
/// </summary>
/// <remarks>
/// No fixture and no broker: validation runs before the bus is registered, so nothing ever connects.
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
                ["OpinionatedFramework:Events:Driver"] = "MyServiceBus.RabbitMq",
                ["OpinionatedFramework:Events:Host"] = "localhost"
            })
            .Build();

        var exception = await Assert.ThrowsAsync<BootstrapConfigurationException>(() =>
            OpinionatedFrameworkBootstrapping.StartAsync(configuration, options =>
                options.MyServiceBusEvents(events => events.Publishes<NotReadableEvent>())));

        // A consumer rebuilds the event the broker delivered, so one that could not be rebuilt would reach a
        // queue and fail there, which is why it is rejected while validating instead. The event is only
        // declared as raised, so this is also what shows the check covers an event no handler reacts to.
        Assert.Contains("tests.not-readable", exception.Message, StringComparison.Ordinal);
        Assert.Contains("cannot be carried by this driver", exception.Message, StringComparison.Ordinal);

        // MyServiceBus says what stopped it from reading the event back, and the error reports what it said
        // rather than describing the failure on its own.
        Assert.Contains("Cannot deserialize message as", exception.Message, StringComparison.Ordinal);
    }
}
