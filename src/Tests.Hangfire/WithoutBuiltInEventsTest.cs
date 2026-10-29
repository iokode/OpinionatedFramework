using System;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.ContractImplementations.Hangfire;
using IOKode.OpinionatedFramework.Events;
using IOKode.OpinionatedFramework.ServiceLocation;
using IOKode.OpinionatedFramework.Tests.Hangfire.Config;
using IOKode.OpinionatedFramework.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace IOKode.OpinionatedFramework.Tests.Hangfire;

/// <summary>
/// Exercises an application that did not opt into the built-in events, which raises no
/// <see cref="EventHandlerExhausted"/> even though it declares a handler of it.
/// </summary>
[Collection(nameof(WithoutBuiltInEventsFixtureCollection))]
public class WithoutBuiltInEventsTest
{
    private static readonly TimeSpan settle = TimeSpan.FromSeconds(5);

    public WithoutBuiltInEventsTest(WithoutBuiltInEventsFixture fixture, ITestOutputHelper output)
    {
        fixture.TestOutputHelperFactory = () => output;
    }

    [Fact]
    public async Task AnApplicationThatDidNotOptInRaisesNothing()
    {
        HandledEvents.Reset();

        // The premise of the test, which a later change to the fixture would otherwise silently break.
        Assert.False(Locator.Resolve<HangfireEventsOptions>().IsDeclaredAsPublished(typeof(EventHandlerExhausted)));

        var dispatcher = Locator.Resolve<IEventDispatcher>();

        await dispatcher.DispatchAsync(new OrderRejected {OrderId = Guid.NewGuid()}, CancellationToken.None);

        bool failed = await PollingUtility.WaitUntilTrueAsync(
            HandledEvents.WasHandledBy<AlwaysFailWithoutRetry>,
            timeout: 60_000,
            pollingInterval: 250);

        Assert.True(failed, "The failing handler did not run within the allowed time.");
        await Task.Delay(settle);

        Assert.Empty(HandledEvents.Exhaustions);
    }
}
