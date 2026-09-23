using System;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Events;
using IOKode.OpinionatedFramework.ServiceLocation;
using IOKode.OpinionatedFramework.Tests.MyServiceBus.RabbitMq.Config;
using IOKode.OpinionatedFramework.Utilities;
using MyServiceBus;
using Xunit;

namespace IOKode.OpinionatedFramework.Tests.MyServiceBus.RabbitMq;

/// <summary>
/// Exercises the broker-backed event dispatcher, where the dispatcher publishes once and the broker copies the
/// event into the queue of every subscribed handler.
/// </summary>
[Collection(nameof(EventsTestsFixtureCollection))]
public class EventDispatcherTest
{
    [Fact]
    public async Task EverySubscribedHandlerReceivesItsOwnCopy()
    {
        var dispatcher = Locator.Resolve<IEventDispatcher>();
        var orderId = Guid.NewGuid();

        await dispatcher.DispatchAsync(
            new OrderSubmitted {OrderId = orderId, Customer = "ada"},
            CancellationToken.None);

        bool handled = await PollingUtility.WaitUntilTrueAsync(
            () => HandledEvents.WasHandledBy<SendConfirmationEmail>(orderId)
                  && HandledEvents.WasHandledBy<UpdateStatistics>(orderId)
                  && HandledEvents.WasHandledBy<StoreEvent>(orderId),
            timeout: 60_000,
            pollingInterval: 250);

        Assert.True(handled, "The subscribed handlers did not receive the event within the allowed time.");
    }

    [Fact]
    public async Task AHandlerSubscribedToTheEventInterfaceReceivesADifferentEvent()
    {
        var dispatcher = Locator.Resolve<IEventDispatcher>();
        var orderId = Guid.NewGuid();

        await dispatcher.DispatchAsync(new OrderCancelled {OrderId = orderId}, CancellationToken.None);

        bool handled = await PollingUtility.WaitUntilTrueAsync(
            () => HandledEvents.WasHandledBy<StoreEvent>(orderId),
            timeout: 60_000,
            pollingInterval: 250);

        Assert.True(handled, "The base-type handler did not receive the event within the allowed time.");
    }

    [Fact]
    public async Task AFailedDeliveryIsRetriedByTheReceiveEndpoint()
    {
        var dispatcher = Locator.Resolve<IEventDispatcher>();
        var orderId = Guid.NewGuid();

        await dispatcher.DispatchAsync(new OrderCancelled {OrderId = orderId}, CancellationToken.None);

        bool handled = await PollingUtility.WaitUntilTrueAsync(
            () => HandledEvents.WasHandledBy<FailOnFirstDelivery>(orderId),
            timeout: 60_000,
            pollingInterval: 250);

        Assert.True(handled, "The handler did not succeed on the retried delivery within the allowed time.");
        Assert.True(HandledEvents.RecoveringAttempts >= 2, "The failed delivery was not retried.");
    }

    [Fact]
    public async Task ASubscribableOnlyEventIsHandledWhenAnotherApplicationRaisesIt()
    {
        var orderId = Guid.NewGuid();

        // This application cannot hand it to the dispatcher — it is not publishable here — so the transport is
        // used directly to stand in for the application that does raise it.
        await Locator.Resolve<IMessageBus>().Publish(new PartnerPayment {OrderId = orderId});

        bool handled = await PollingUtility.WaitUntilTrueAsync(
            () => HandledEvents.WasHandledBy<HandlePartnerPayment>(orderId),
            timeout: 60_000,
            pollingInterval: 250);

        Assert.True(handled, "The handler of an event this application never raises did not receive it.");
    }

    [Fact]
    public async Task APublishableOnlyEventIsRaisedAndTheBusKeepsWorking()
    {
        var dispatcher = Locator.Resolve<IEventDispatcher>();

        // Nothing can be registered for it, so it reaches no queue of this application.
        await dispatcher.DispatchAsync(new AuditRecorded {OrderId = Guid.NewGuid()}, CancellationToken.None);

        // A symmetric event afterwards still flows, which is what proves the previous one broke nothing.
        var orderId = Guid.NewGuid();
        await dispatcher.DispatchAsync(
            new OrderSubmitted {OrderId = orderId, Customer = "ada"},
            CancellationToken.None);

        bool handled = await PollingUtility.WaitUntilTrueAsync(
            () => HandledEvents.WasHandledBy<SendConfirmationEmail>(orderId),
            timeout: 60_000,
            pollingInterval: 250);

        Assert.True(handled, "The bus stopped working after an event nothing subscribes to.");
    }

    [Fact]
    public async Task AnEventDeclaredOnlyWithAddEventReachesTheInterfaceHandler()
    {
        var dispatcher = Locator.Resolve<IEventDispatcher>();
        var orderId = Guid.NewGuid();

        // No handler names this event: it is subscribed only because the interface registration was expanded
        // over the concrete types the driver was told about.
        await dispatcher.DispatchAsync(new InventoryAdjusted {OrderId = orderId}, CancellationToken.None);

        bool handled = await PollingUtility.WaitUntilTrueAsync(
            () => HandledEvents.WasHandledBy<StoreEvent>(orderId),
            timeout: 60_000,
            pollingInterval: 250);

        Assert.True(handled, "The event declared with AddEvent did not reach the interface handler.");
    }
}
