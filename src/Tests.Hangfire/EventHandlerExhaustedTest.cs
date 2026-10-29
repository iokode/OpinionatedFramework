using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using IOKode.OpinionatedFramework.ContractImplementations.Hangfire;
using IOKode.OpinionatedFramework.Events;
using IOKode.OpinionatedFramework.ServiceLocation;
using IOKode.OpinionatedFramework.Tests.Hangfire.Config;
using IOKode.OpinionatedFramework.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace IOKode.OpinionatedFramework.Tests.Hangfire;

/// <summary>
/// Exercises when the Hangfire event dispatcher raises <see cref="EventHandlerExhausted"/>, which is when an
/// event handler job is left in the failed state.
/// </summary>
/// <remarks>
/// Every assertion that something was not raised, or was raised only once, waits first: the handlers and the
/// exhaustion event run in jobs on the server, so an absence right after dispatching would prove nothing.
/// </remarks>
[Collection(nameof(JobsTestsFixtureCollection))]
public class EventHandlerExhaustedTest(JobsTestsFixture fixture, ITestOutputHelper output)
    : JobsTestsBase(fixture, output)
{
    private static readonly TimeSpan settle = TimeSpan.FromSeconds(5);

    [Fact]
    public async Task AHandlerThatExhaustsItsRetriesRaisesTheEventOnce()
    {
        HandledEvents.Reset();
        var dispatcher = Locator.Resolve<IEventDispatcher>();
        var orderId = Guid.NewGuid();

        await dispatcher.DispatchAsync(new OrderCancelled {OrderId = orderId}, CancellationToken.None);

        bool raised = await PollingUtility.WaitUntilTrueAsync(
            () => ExhaustionsOf<AlwaysFail>(orderId).Length > 0,
            timeout: 60_000,
            pollingInterval: 250);

        Assert.True(raised, "The exhaustion was not raised within the allowed time.");
        await Task.Delay(settle);

        var exhausted = Assert.Single(ExhaustionsOf<AlwaysFail>(orderId));
        Assert.Equal("tests.order-cancelled", exhausted.EventName);

        // The first attempt and the two retries the policy declares.
        Assert.Equal(3, exhausted.Attempts);
    }

    [Fact]
    public async Task ARequeuedJobThatFailsAgainCountsOnlyItsOwnRound()
    {
        HandledEvents.Reset();
        var dispatcher = Locator.Resolve<IEventDispatcher>();
        var orderId = Guid.NewGuid();

        await dispatcher.DispatchAsync(new OrderCancelled {OrderId = orderId}, CancellationToken.None);

        bool exhausted = await PollingUtility.WaitUntilTrueAsync(
            () => ExhaustionsOf<AlwaysFail>(orderId).Length > 0,
            timeout: 60_000,
            pollingInterval: 250);

        Assert.True(exhausted, "The first exhaustion was not raised within the allowed time.");

        var storage = Locator.Resolve<JobStorage>();
        new BackgroundJobClient(storage).Requeue(FailedJobOf<AlwaysFail>(storage, orderId));

        bool exhaustedAgain = await PollingUtility.WaitUntilTrueAsync(
            () => ExhaustionsOf<AlwaysFail>(orderId).Length > 1,
            timeout: 60_000,
            pollingInterval: 250);

        Assert.True(exhaustedAgain, "The second exhaustion was not raised within the allowed time.");
        await Task.Delay(settle);

        // Hangfire keeps the retry count across the requeue, so the job had its retries used up and got a single
        // attempt, which is all the second exhaustion reports.
        var attempts = ExhaustionsOf<AlwaysFail>(orderId).Select(exhaustion => exhaustion.Attempts).Order();
        Assert.Equal([1, 3], attempts);
    }

    [Fact]
    public async Task AHandlerWithoutRetriesIsExhaustedByItsFirstFailure()
    {
        HandledEvents.Reset();
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
        HandledEvents.Reset();
        var dispatcher = Locator.Resolve<IEventDispatcher>();
        var orderId = Guid.NewGuid();

        await dispatcher.DispatchAsync(
            new OrderSubmitted {OrderId = orderId, Customer = "ada"},
            CancellationToken.None);

        bool handled = await PollingUtility.WaitUntilTrueAsync(
            HandledEvents.WasHandledBy<FailOnFirstAttempt>,
            timeout: 60_000,
            pollingInterval: 250);

        Assert.True(handled, "The handler did not succeed on its retry within the allowed time.");
        await Task.Delay(settle);

        Assert.DoesNotContain(HandledEvents.Exhaustions, exhaustion => exhaustion.OrderId == orderId);
    }

    [Fact]
    public async Task AFailingHandlerOfTheEventDoesNotRaiseItAgain()
    {
        HandledEvents.Reset();
        var dispatcher = Locator.Resolve<IEventDispatcher>();

        await dispatcher.DispatchAsync(new OrderRejected {OrderId = Guid.NewGuid()}, CancellationToken.None);

        bool failed = await PollingUtility.WaitUntilTrueAsync(
            HandledEvents.WasHandledBy<FailOnExhaustion>,
            timeout: 60_000,
            pollingInterval: 250);

        Assert.True(failed, "The failing handler of the exhaustion event did not run within the allowed time.");
        await Task.Delay(settle);

        Assert.DoesNotContain(HandledEvents.Exhaustions,
            exhaustion => exhaustion.HandlerName == typeof(FailOnExhaustion).FullName);
    }

    private static string FailedJobOf<THandler>(JobStorage storage, Guid orderId) =>
        storage.GetMonitoringApi().FailedJobs(0, int.MaxValue)
            .Single(failed => failed.Value.Job.Args.OfType<ExecuteEventHandlerJobCreator>().Any(creator =>
                creator.HandlerTypeName == typeof(THandler).FullName
                && creator.EventBody.Contains(orderId.ToString())))
            .Key;

    private static Exhaustion[] ExhaustionsOf<THandler>(Guid orderId) =>
        HandledEvents.Exhaustions
            .Where(exhaustion => exhaustion.HandlerName == typeof(THandler).FullName
                                 && exhaustion.OrderId == orderId)
            .ToArray();
}
