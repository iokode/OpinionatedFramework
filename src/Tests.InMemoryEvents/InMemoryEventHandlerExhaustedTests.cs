using System;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.ContractImplementations.InMemoryEvents;
using IOKode.OpinionatedFramework.Events;
using IOKode.OpinionatedFramework.Events.Extensions;
using IOKode.OpinionatedFramework.ServiceContainer;
using IOKode.OpinionatedFramework.ServiceLocation;
using IOKode.OpinionatedFramework.Tests.InMemoryEvents.Config;
using IOKode.OpinionatedFramework.Utilities;
using Xunit;

namespace IOKode.OpinionatedFramework.Tests.InMemoryEvents;

/// <summary>
/// Exercises when the in-memory driver raises <see cref="EventHandlerExhausted"/>.
/// </summary>
/// <remarks>
/// Every assertion that something was not raised, or was raised only once, waits first: the handlers run on the
/// workers of the dispatcher, so an absence right after dispatching would prove nothing.
/// </remarks>
[Collection(InMemoryEventsCollection.Name)]
public class InMemoryEventHandlerExhaustedTests : IAsyncLifetime
{
    private static readonly TimeSpan settle = TimeSpan.FromMilliseconds(500);

    public async Task InitializeAsync()
    {
        await Container.Advanced.ResetAsync();
        HandledEvents.Reset();
    }

    public async Task DisposeAsync()
    {
        await Container.Advanced.ResetAsync();
    }

    [Fact]
    public async Task A_handler_that_exhausts_its_retries_raises_the_event_once()
    {
        Configure(events =>
        {
            events.PublishesBuiltInEvents();
            events.Publishes<OrderCancelled>();
            events.Handles<OrderCancelled, AlwaysFail>(policy => policy.Retry(2));
            events.Handles<EventHandlerExhausted, RecordExhaustion>();
        });

        var orderId = Guid.NewGuid();
        await DispatchAsync(new OrderCancelled {OrderId = orderId});

        Assert.True(await WaitForAsync(() => HandledEvents.Exhaustions.Count > 0));
        await Task.Delay(settle);

        var exhausted = Assert.Single(HandledEvents.Exhaustions);
        Assert.Equal("tests.order-cancelled", exhausted.EventName);
        Assert.Equal(typeof(AlwaysFail).FullName, exhausted.HandlerName);
        Assert.Equal(3, exhausted.Attempts);

        // The failed event travels as its JSON, which is readable without declaring the event it came from.
        Assert.Equal(orderId, exhausted.EventBody.RootElement.GetProperty("orderId").GetGuid());
    }

    [Fact]
    public async Task A_handler_without_retries_is_exhausted_by_its_first_failure()
    {
        Configure(events =>
        {
            events.PublishesBuiltInEvents();
            events.Publishes<OrderCancelled>();
            events.Handles<OrderCancelled, AlwaysFail>();
            events.Handles<EventHandlerExhausted, RecordExhaustion>();
        });

        await DispatchAsync(new OrderCancelled {OrderId = Guid.NewGuid()});

        Assert.True(await WaitForAsync(() => HandledEvents.Exhaustions.Count > 0));
        await Task.Delay(settle);

        var exhausted = Assert.Single(HandledEvents.Exhaustions);
        Assert.Equal(1, exhausted.Attempts);
    }

    [Fact]
    public async Task A_retry_that_succeeds_raises_nothing()
    {
        Configure(events =>
        {
            events.PublishesBuiltInEvents();
            events.Publishes<OrderSubmitted>();
            events.Handles<OrderSubmitted, FailOnFirstAttempt>(policy => policy.Retry(1));
            events.Handles<EventHandlerExhausted, RecordExhaustion>();
        });

        await DispatchAsync(new OrderSubmitted {OrderId = Guid.NewGuid(), Customer = "ada"});

        Assert.True(await WaitForAsync(HandledEvents.WasHandledBy<FailOnFirstAttempt>));
        await Task.Delay(settle);

        Assert.Empty(HandledEvents.Exhaustions);
    }

    [Fact]
    public async Task A_failing_handler_of_the_event_does_not_raise_it_again()
    {
        Configure(events =>
        {
            events.PublishesBuiltInEvents();
            events.Publishes<OrderCancelled>();
            events.Handles<OrderCancelled, AlwaysFail>();
            events.Handles<EventHandlerExhausted, FailOnExhaustion>();
            events.Handles<EventHandlerExhausted, RecordExhaustion>();
        });

        await DispatchAsync(new OrderCancelled {OrderId = Guid.NewGuid()});

        Assert.True(await WaitForAsync(HandledEvents.WasHandledBy<FailOnExhaustion>));
        await Task.Delay(settle);

        // The only exhaustion is the one of the handler of the cancelled order, not one of the handler that
        // failed on the exhaustion event itself.
        var exhausted = Assert.Single(HandledEvents.Exhaustions);
        Assert.Equal(typeof(AlwaysFail).FullName, exhausted.HandlerName);
        Assert.Equal(1, HandledEvents.CountFor<FailOnExhaustion>());
    }

    [Fact]
    public async Task An_application_that_did_not_opt_in_raises_nothing()
    {
        Configure(events =>
        {
            // Reacting to the event is declared; raising it is not.
            events.Publishes<OrderCancelled>();
            events.Handles<OrderCancelled, AlwaysFail>();
            events.Handles<EventHandlerExhausted, RecordExhaustion>();
        });

        await DispatchAsync(new OrderCancelled {OrderId = Guid.NewGuid()});

        Assert.True(await WaitForAsync(() => HandledEvents.Attempts > 0));
        await Task.Delay(settle);

        Assert.Empty(HandledEvents.Exhaustions);
    }

    private static void Configure(Action<InMemoryEventsOptions> configure)
    {
        Container.Services.AddInMemoryEventDispatcher(configure);
        Container.Initialize();
    }

    private static async Task DispatchAsync(IPublishableEvent @event)
    {
        var dispatcher = Locator.Resolve<IEventDispatcher>();
        await dispatcher.DispatchAsync(@event, CancellationToken.None);
    }

    private static async Task<bool> WaitForAsync(Func<bool> condition) =>
        await PollingUtility.WaitUntilTrueAsync(condition, timeout: 10_000, pollingInterval: 25);
}
