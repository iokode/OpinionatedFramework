using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Bootstrapping;
using IOKode.OpinionatedFramework.ContractImplementations.InMemoryEvents;
using IOKode.OpinionatedFramework.Drivers.Abstractions;
using IOKode.OpinionatedFramework.Events;
using IOKode.OpinionatedFramework.ServiceContainer;
using IOKode.OpinionatedFramework.ServiceContainer.Drivers;
using IOKode.OpinionatedFramework.Tests.InMemoryEvents.Config;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace IOKode.OpinionatedFramework.Tests.InMemoryEvents;

/// <summary>
/// Covers the rule that no two event types may declare the same name.
/// </summary>
/// <remarks>
/// The rule itself is shared by the three drivers, so it is exercised here through the one that needs no
/// infrastructure. What each driver adds is the wiring, which is one call in its registration and one in its
/// bootstrap validation.
/// </remarks>
[Collection(InMemoryEventsCollection.Name)]
public class EventNameUniquenessTests : IAsyncLifetime
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
    public void Registration_rejects_two_event_types_declaring_the_same_name()
    {
        var exception = Assert.Throws<DuplicateEventNameException>(() =>
            Container.Services.AddInMemoryEventDispatcher(events =>
            {
                events.AddEventHandler<OrderSubmitted, SendConfirmationEmail>();
                events.AddEventHandler<DuplicateOrderSubmitted, HandleDuplicate>();
            }));

        Assert.Contains("tests.order-submitted", exception.Message, StringComparison.Ordinal);
        Assert.Contains(nameof(DuplicateOrderSubmitted), exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Bootstrap_validation_reports_two_event_types_declaring_the_same_name()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OpinionatedFramework:Events:Driver"] = "in-memory"
            })
            .Build();

        var exception = await Assert.ThrowsAsync<BootstrapConfigurationException>(() =>
            OpinionatedFrameworkBootstrapping.StartAsync(configuration, options =>
                options.InMemoryEvents(events =>
                {
                    events.AddEventHandler<OrderSubmitted, SendConfirmationEmail>();
                    events.AddEventHandler<DuplicateOrderSubmitted, HandleDuplicate>();
                })));

        // Reported as a configuration error, so the application never starts with an ambiguous event identity.
        Assert.Contains("tests.order-submitted", exception.Message, StringComparison.Ordinal);
    }
}
