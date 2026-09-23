using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Bootstrapping;
using IOKode.OpinionatedFramework.ContractImplementations.InMemoryEvents;
using IOKode.OpinionatedFramework.Events;
using IOKode.OpinionatedFramework.ServiceContainer;
using IOKode.OpinionatedFramework.ServiceLocation;
using IOKode.OpinionatedFramework.Tests.InMemoryEvents.Config;
using IOKode.OpinionatedFramework.Utilities;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace IOKode.OpinionatedFramework.Tests.InMemoryEvents;

/// <summary>
/// Selects the driver the way an application does, through configuration and the bootstrap verb, rather than by
/// registering the implementation directly.
/// </summary>
[Collection(InMemoryEventsCollection.Name)]
public class InMemoryEventsBootstrapTests : IAsyncLifetime
{
    private HostHandle? host;

    public async Task InitializeAsync()
    {
        await Container.Advanced.ResetAsync();
        HandledEvents.Reset();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OpinionatedFramework:Events:Driver"] = "in-memory"
            })
            .Build();

        this.host = await OpinionatedFrameworkBootstrapping.StartAsync(configuration, options =>
            options.InMemoryEvents(events => events.AddEventHandler<OrderSubmitted, SendConfirmationEmail>()));
    }

    public async Task DisposeAsync()
    {
        if (this.host is not null)
        {
            await this.host.DisposeAsync();
        }

        await Container.Advanced.ResetAsync();
    }

    [Fact]
    public async Task The_configured_driver_handles_the_event()
    {
        var dispatcher = Locator.Resolve<IEventDispatcher>();
        Assert.IsType<InMemoryEventDispatcher>(dispatcher);

        await dispatcher.DispatchAsync(
            new OrderSubmitted {OrderId = Guid.NewGuid(), Customer = "ada"},
            CancellationToken.None);

        var handled = await PollingUtility.WaitUntilTrueAsync(
            HandledEvents.WasHandledBy<SendConfirmationEmail>,
            timeout: 10_000,
            pollingInterval: 25);

        Assert.True(handled, "The handler declared through the bootstrap verb did not run.");
    }
}
