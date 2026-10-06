using System;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.ContractImplementations.MyServiceBus;
using IOKode.OpinionatedFramework.Events;
using IOKode.OpinionatedFramework.ServiceLocation;
using IOKode.OpinionatedFramework.Tests.MyServiceBus.RabbitMq.Config;
using IOKode.OpinionatedFramework.Utilities;
using Xunit;

namespace IOKode.OpinionatedFramework.Tests.MyServiceBus.RabbitMq;

/// <summary>
/// Exercises a process that only emits: it declares no handler, so its bus has no receive endpoint, and what
/// reacts to the event is a different application.
/// </summary>
[Collection(nameof(PublisherOnlyFixtureCollection))]
public class PublisherOnlyEventDispatcherTest
{
    [Fact]
    public async Task AnApplicationThatDeclaresNoHandlerStillEmitsToTheApplicationThatReacts()
    {
        // The premise of the test, which a later change to the fixture would otherwise silently break.
        Assert.Empty(Locator.Resolve<MyServiceBusEventsOptions>().EventHandlerRegistrations);

        var dispatcher = Locator.Resolve<IEventDispatcher>();
        var orderId = Guid.NewGuid();

        await dispatcher.DispatchAsync(new AuditRecorded {OrderId = orderId}, CancellationToken.None);

        bool received = await PollingUtility.WaitUntilTrueAsync(
            () => ExternalSubscriber.Received(orderId),
            timeout: 60_000,
            pollingInterval: 250);

        Assert.True(received,
            "The application that subscribes the event did not receive it within the allowed time.");
    }
}
