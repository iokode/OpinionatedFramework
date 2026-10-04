using System;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Events;
using IOKode.OpinionatedFramework.ServiceLocation;
using IOKode.OpinionatedFramework.Tests.Hangfire.Config;
using IOKode.OpinionatedFramework.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace IOKode.OpinionatedFramework.Tests.Hangfire;

/// <summary>
/// Exercises the Hangfire event dispatcher, where dispatching stores one job per handler and the handlers run
/// later on the server draining the events queue.
/// </summary>
[Collection(nameof(JobsTestsFixtureCollection))]
public class EventDispatcherTest(JobsTestsFixture fixture, ITestOutputHelper output)
    : JobsTestsBase(fixture, output)
{
    [Fact]
    public async Task EachHandlerRunsFromItsOwnJob()
    {
        HandledEvents.Reset();
        var dispatcher = Locator.Resolve<IEventDispatcher>();

        // Dispatch returns as soon as the jobs are stored, so the handlers have not run yet at this point.
        await dispatcher.DispatchAsync(
            new OrderSubmitted {OrderId = Guid.NewGuid(), Customer = "ada"},
            CancellationToken.None);

        bool handled = await PollingUtility.WaitUntilTrueAsync(
            () => HandledEvents.WasHandledBy<SendConfirmationEmail>()
                  && HandledEvents.WasHandledBy<UpdateStatistics>()
                  && HandledEvents.WasHandledBy<FailOnFirstAttempt>(),
            timeout: 60_000,
            pollingInterval: 250);

        Assert.True(handled, "The handlers did not run within the allowed time.");

        // The handler that failed once succeeded on the retry Hangfire scheduled for it.
        Assert.Equal(2, HandledEvents.RecoveringAttempts);
    }

    [Fact]
    public async Task TheDeclaredRetryCountReplacesTheHangfireDefault()
    {
        HandledEvents.Reset();
        var dispatcher = Locator.Resolve<IEventDispatcher>();

        await dispatcher.DispatchAsync(new OrderCancelled {OrderId = Guid.NewGuid()}, CancellationToken.None);

        // Three attempts within seconds is only reachable with the one-second delay this handler declares:
        // Hangfire's own default would still be waiting for its first retry.
        bool exhausted = await PollingUtility.WaitUntilTrueAsync(
            () => HandledEvents.FailingAttempts >= 3,
            timeout: 60_000,
            pollingInterval: 250);

        Assert.True(exhausted, "The handler did not exhaust its declared attempts within the allowed time.");

        // And it stops there rather than continuing with Hangfire's ten default attempts.
        await Task.Delay(TimeSpan.FromSeconds(5));
        Assert.Equal(3, HandledEvents.FailingAttempts);
    }

    [Fact]
    public async Task APublishableOnlyEventEnqueuesNoJob()
    {
        HandledEvents.Reset();
        var dispatcher = Locator.Resolve<IEventDispatcher>();

        // Nothing can be registered for it, so the fan-out produces no job at all.
        await dispatcher.DispatchAsync(new AuditRecorded {OrderId = Guid.NewGuid()}, CancellationToken.None);

        // Dispatching a symmetric event afterwards proves the server is draining the queue, so the absence
        // below is a real absence and not a slow start.
        await dispatcher.DispatchAsync(
            new OrderSubmitted {OrderId = Guid.NewGuid(), Customer = "ada"},
            CancellationToken.None);

        bool handled = await PollingUtility.WaitUntilTrueAsync(
            () => HandledEvents.WasHandledBy<SendConfirmationEmail>(),
            timeout: 60_000,
            pollingInterval: 250);

        Assert.True(handled, "The symmetric event did not run within the allowed time.");

        // No handler ran for the publishable-only event, because none could be registered for it.
        Assert.False(HandledEvents.AnyHandlerRanFor("tests.audit-recorded"));
    }
}
