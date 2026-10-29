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
/// Exercises an application that did not opt into the built-in events, which raises no
/// <see cref="EventHandlerExhausted"/> even though it declares a handler of it.
/// </summary>
[Collection(nameof(WithoutBuiltInEventsFixtureCollection))]
public class WithoutBuiltInEventsTest
{
    private static readonly TimeSpan settle = TimeSpan.FromSeconds(3);

    [Fact]
    public async Task AnApplicationThatDidNotOptInRaisesNothing()
    {
        // The premise of the test, which a later change to the fixture would otherwise silently break.
        Assert.False(Locator.Resolve<MyServiceBusEventsOptions>()
            .IsDeclaredAsPublished(typeof(EventHandlerExhausted)));

        var dispatcher = Locator.Resolve<IEventDispatcher>();
        var orderId = Guid.NewGuid();

        await dispatcher.DispatchAsync(new OrderRejected {OrderId = orderId}, CancellationToken.None);

        bool failed = await PollingUtility.WaitUntilTrueAsync(
            () => HandledEvents.WasHandledBy<AlwaysFailWithoutRetry>(orderId),
            timeout: 60_000,
            pollingInterval: 250);

        Assert.True(failed, "The failing handler did not run within the allowed time.");
        await Task.Delay(settle);

        Assert.DoesNotContain(HandledEvents.Exhaustions, exhaustion => exhaustion.OrderId == orderId);
    }
}
