using System;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Events;
using IOKode.OpinionatedFramework.ServiceLocation;
using IOKode.OpinionatedFramework.Tests.MyServiceBus.RabbitMq.Config;
using IOKode.OpinionatedFramework.Utilities;
using Xunit;
using PartnerApplication = IOKode.OpinionatedFramework.Tests.MyServiceBus.RabbitMq.Config.PartnerApplication;

namespace IOKode.OpinionatedFramework.Tests.MyServiceBus.RabbitMq;

/// <summary>
/// Exercises two applications that share the declared name of an event and not its type.
/// </summary>
/// <remarks>
/// This is what the declared name being the wire identity buys: each application declares the event as a type
/// of its own, in its own namespace, with its own members, and the broker still carries one application's event
/// to the other. Nothing is shared between the two but the name in <c>[EventName]</c> and the members the
/// payload holds.
/// </remarks>
[Collection(nameof(EventsTestsFixtureCollection))]
public class CrossApplicationEventTest(EventsTestsFixture fixture)
{
    [Fact]
    public async Task AnEventRaisedByAnotherApplicationReachesTheHandlerOfTheTypeDeclaredHere()
    {
        var orderId = Guid.NewGuid();

        await fixture.PartnerBus.Publish(new PartnerApplication.PartnerPayment {OrderId = orderId});

        bool handled = await PollingUtility.WaitUntilTrueAsync(
            () => HandledEvents.WasHandledBy<HandlePartnerPayment>(orderId),
            timeout: 60_000,
            pollingInterval: 250);

        Assert.True(handled, "The event raised by the other application did not reach the handler declared here.");
    }

    [Fact]
    public async Task AnEventRaisedHereReachesAnotherApplicationThatDeclaresItsOwnType()
    {
        var dispatcher = Locator.Resolve<IEventDispatcher>();
        var orderId = Guid.NewGuid();

        await dispatcher.DispatchAsync(
            new OrderSubmitted {OrderId = orderId, Customer = "ada"},
            CancellationToken.None);

        bool received = await PollingUtility.WaitUntilTrueAsync(
            () => PartnerApplication.PartnerSubscriber.Received(orderId),
            timeout: 60_000,
            pollingInterval: 250);

        Assert.True(received, "The other application did not receive the event raised here.");
    }
}
