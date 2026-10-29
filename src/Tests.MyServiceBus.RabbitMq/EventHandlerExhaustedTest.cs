using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.ContractImplementations.MyServiceBus;
using IOKode.OpinionatedFramework.Events;
using IOKode.OpinionatedFramework.ServiceLocation;
using IOKode.OpinionatedFramework.Tests.MyServiceBus.RabbitMq.Config;
using IOKode.OpinionatedFramework.Utilities;
using MyServiceBus;
using Xunit;

namespace IOKode.OpinionatedFramework.Tests.MyServiceBus.RabbitMq;

/// <summary>
/// Exercises when the broker-backed event dispatcher raises <see cref="EventHandlerExhausted"/>, which is when a
/// failure gets past the retry filter of a handler's pipe.
/// </summary>
/// <remarks>
/// The exhaustion event is published to the broker like any other and read back by its handlers from their own
/// queues, so what is asserted is what survived the wire.
/// </remarks>
/// <remarks>
/// Every assertion that something was not raised, or was raised only once, waits first: the handlers run when
/// the broker delivers, so an absence right after dispatching would prove nothing.
/// </remarks>
[Collection(nameof(EventsTestsFixtureCollection))]
public class EventHandlerExhaustedTest
{
    private static readonly TimeSpan settle = TimeSpan.FromSeconds(3);

    [Fact]
    public async Task AHandlerThatExhaustsItsRetriesRaisesTheEventOnce()
    {
        var dispatcher = Locator.Resolve<IEventDispatcher>();
        var orderId = Guid.NewGuid();

        await dispatcher.DispatchAsync(new OrderRejected {OrderId = orderId}, CancellationToken.None);

        bool raised = await PollingUtility.WaitUntilTrueAsync(
            () => ExhaustionsOf<AlwaysFail>(orderId).Length > 0,
            timeout: 60_000,
            pollingInterval: 250);

        Assert.True(raised, "The exhaustion was not raised within the allowed time.");
        await Task.Delay(settle);

        var exhausted = Assert.Single(ExhaustionsOf<AlwaysFail>(orderId));
        Assert.Equal("tests.order-rejected", exhausted.EventName);

        // The first attempt and the two retries the policy declares.
        Assert.Equal(3, exhausted.Attempts);
    }

    [Fact]
    public async Task AHandlerWithoutRetriesIsExhaustedByItsFirstFailure()
    {
        var dispatcher = Locator.Resolve<IEventDispatcher>();
        var orderId = Guid.NewGuid();

        await dispatcher.DispatchAsync(new OrderRejected {OrderId = orderId}, CancellationToken.None);

        bool raised = await PollingUtility.WaitUntilTrueAsync(
            () => ExhaustionsOf<AlwaysFailWithoutRetry>(orderId).Length > 0,
            timeout: 60_000,
            pollingInterval: 250);

        Assert.True(raised, "The exhaustion was not raised within the allowed time.");
        await Task.Delay(settle);

        var exhausted = Assert.Single(ExhaustionsOf<AlwaysFailWithoutRetry>(orderId));
        Assert.Equal(1, exhausted.Attempts);
    }

    [Fact]
    public async Task ARetryThatSucceedsRaisesNothing()
    {
        var dispatcher = Locator.Resolve<IEventDispatcher>();
        var orderId = Guid.NewGuid();

        await dispatcher.DispatchAsync(new OrderCancelled {OrderId = orderId}, CancellationToken.None);

        bool handled = await PollingUtility.WaitUntilTrueAsync(
            () => HandledEvents.WasHandledBy<FailOnFirstDelivery>(orderId),
            timeout: 60_000,
            pollingInterval: 250);

        Assert.True(handled, "The handler did not succeed on its retry within the allowed time.");
        await Task.Delay(settle);

        Assert.DoesNotContain(HandledEvents.Exhaustions, exhaustion => exhaustion.OrderId == orderId);
    }

    [Fact]
    public async Task AFailingHandlerOfTheEventDoesNotRaiseItAgain()
    {
        var dispatcher = Locator.Resolve<IEventDispatcher>();

        await dispatcher.DispatchAsync(new OrderRejected {OrderId = Guid.NewGuid()}, CancellationToken.None);

        bool failed = await PollingUtility.WaitUntilTrueAsync(
            () => HandledEvents.WasHandledBy<FailOnExhaustion>(Guid.Empty),
            timeout: 60_000,
            pollingInterval: 250);

        Assert.True(failed, "The failing handler of the exhaustion event did not run within the allowed time.");
        await Task.Delay(settle);

        Assert.DoesNotContain(HandledEvents.Exhaustions,
            exhaustion => exhaustion.HandlerName == typeof(FailOnExhaustion).FullName);
    }

    [Fact]
    public async Task ACancelledDeliveryRaisesNothing()
    {
        var orderId = Guid.NewGuid();
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        // What a handler in flight leaves with when the bus stops: the token of its delivery is cancelled.
        var filter = new EventHandlerExhaustionFilter<OrderRejected>(typeof(AlwaysFail), attempts: 3);
        await Assert.ThrowsAsync<OperationCanceledException>(() => filter.Send(
            new FakeConsumeContext<OrderRejected>(new OrderRejected {OrderId = orderId}, cancellation.Token),
            new FailingPipe<ConsumeContext<OrderRejected>>(new OperationCanceledException(cancellation.Token))));

        await Task.Delay(settle);

        Assert.Empty(ExhaustionsOf<AlwaysFail>(orderId));
    }

    [Fact]
    public async Task AHandlerThatThrowsCancellationOnItsOwnIsExhausted()
    {
        var orderId = Guid.NewGuid();

        // A timeout of the handler's own: the exception is a cancellation, the delivery was not cancelled.
        var filter = new EventHandlerExhaustionFilter<OrderRejected>(typeof(AlwaysFail), attempts: 3);
        await Assert.ThrowsAsync<OperationCanceledException>(() => filter.Send(
            new FakeConsumeContext<OrderRejected>(new OrderRejected {OrderId = orderId}, CancellationToken.None),
            new FailingPipe<ConsumeContext<OrderRejected>>(new OperationCanceledException())));

        bool raised = await PollingUtility.WaitUntilTrueAsync(
            () => ExhaustionsOf<AlwaysFail>(orderId).Length > 0,
            timeout: 60_000,
            pollingInterval: 250);

        Assert.True(raised, "The exhaustion was not raised within the allowed time.");
        await Task.Delay(settle);

        var exhausted = Assert.Single(ExhaustionsOf<AlwaysFail>(orderId));
        Assert.Equal(3, exhausted.Attempts);
    }

    private static Exhaustion[] ExhaustionsOf<THandler>(Guid orderId) =>
        HandledEvents.Exhaustions
            .Where(exhaustion => exhaustion.HandlerName == typeof(THandler).FullName
                                 && exhaustion.OrderId == orderId)
            .ToArray();
}
