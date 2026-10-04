using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.ContractImplementations.InMemoryEvents;
using IOKode.OpinionatedFramework.Events;
using IOKode.OpinionatedFramework.ServiceContainer;
using IOKode.OpinionatedFramework.ServiceLocation;
using IOKode.OpinionatedFramework.Tests.InMemoryEvents.Config;
using IOKode.OpinionatedFramework.Utilities;
using Xunit;

namespace IOKode.OpinionatedFramework.Tests.InMemoryEvents;

/// <summary>
/// Exercises the in-memory driver, which needs no infrastructure and runs handlers on its own workers rather
/// than in the call that dispatched.
/// </summary>
[Collection(InMemoryEventsCollection.Name)]
public class InMemoryEventDispatcherTests : IAsyncLifetime
{
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
    public async Task Dispatching_does_not_wait_for_the_handlers()
    {
        Configure(events => events.AddEventHandler<OrderSubmitted, SlowHandler>());

        await DispatchAsync(new OrderSubmitted {OrderId = Guid.NewGuid(), Customer = "ada"});

        // The handler sleeps, so seeing it unfinished right after dispatching proves the caller was not held.
        Assert.False(HandledEvents.WasHandledBy<SlowHandler>());
        Assert.True(await WaitForAsync(HandledEvents.WasHandledBy<SlowHandler>));
    }

    [Fact]
    public async Task An_event_that_is_both_publishable_and_subscribable_reaches_every_handler()
    {
        Configure(events =>
        {
            events.AddEventHandler<OrderSubmitted, SendConfirmationEmail>();
            events.AddEventHandler<OrderSubmitted, UpdateStatistics>();
        });

        await DispatchAsync(new OrderSubmitted {OrderId = Guid.NewGuid(), Customer = "ada"});

        Assert.True(await WaitForAsync(() => HandledEvents.WasHandledBy<SendConfirmationEmail>()
                                             && HandledEvents.WasHandledBy<UpdateStatistics>()));
    }

    [Fact]
    public async Task A_handler_registered_for_the_event_interface_receives_every_subscribable_event()
    {
        Configure(events => events.AddEventHandler<ISubscribableEvent, StoreEvent>());

        await DispatchAsync(new OrderSubmitted {OrderId = Guid.NewGuid(), Customer = "ada"});
        await DispatchAsync(new OrderCancelled {OrderId = Guid.NewGuid()});

        Assert.True(await WaitForAsync(() => HandledEvents.CountFor<StoreEvent>() == 2));
    }

    [Fact]
    public async Task A_handler_is_retried_as_its_policy_declares()
    {
        Configure(events => events.AddEventHandler<OrderSubmitted, FailOnFirstAttempt>(policy => policy.Retry(1)));

        await DispatchAsync(new OrderSubmitted {OrderId = Guid.NewGuid(), Customer = "ada"});

        Assert.True(await WaitForAsync(HandledEvents.WasHandledBy<FailOnFirstAttempt>));
        Assert.Equal(2, HandledEvents.Attempts);
    }

    [Fact]
    public async Task A_failing_handler_does_not_stop_the_others()
    {
        Configure(events =>
        {
            events.AddEventHandler<OrderCancelled, AlwaysFail>();
            events.AddEventHandler<OrderCancelled, RecordCancellation>();
        });

        // Nothing is thrown at the caller: the handlers are no longer running in this call.
        await DispatchAsync(new OrderCancelled {OrderId = Guid.NewGuid()});

        Assert.True(await WaitForAsync(HandledEvents.WasHandledBy<RecordCancellation>));
    }

    [Fact]
    public async Task The_configured_worker_count_bounds_how_many_handlers_run_at_once()
    {
        Configure(events =>
        {
            events.SetWorkerCount(1);
            events.AddEventHandler<OrderSubmitted, ConcurrencyProbe>();
        });

        await DispatchAsync(new OrderSubmitted {OrderId = Guid.NewGuid(), Customer = "ada"});
        await DispatchAsync(new OrderSubmitted {OrderId = Guid.NewGuid(), Customer = "grace"});

        Assert.True(await WaitForAsync(() => HandledEvents.CountFor<ConcurrencyProbe>() == 2));

        // One worker drains the queue, so the two executions never overlapped.
        Assert.Equal(1, HandledEvents.MaxConcurrent);
    }

    [Fact]
    public async Task A_publishable_only_event_is_dispatched_and_reaches_nothing()
    {
        Configure(events =>
        {
            events.AddEventHandler<OrderSubmitted, SendConfirmationEmail>();
            events.AddEventHandler<ISubscribableEvent, StoreEvent>();
        });

        // Nothing can be registered for it, and the handler that observes everything subscribable does not
        // reach it either, because it is not subscribable.
        await DispatchAsync(new AuditRecorded {OrderId = Guid.NewGuid()});
        await DispatchAsync(new OrderSubmitted {OrderId = Guid.NewGuid(), Customer = "ada"});

        // Waiting for the symmetric event proves the queue was drained past the publishable-only one.
        Assert.True(await WaitForAsync(HandledEvents.WasHandledBy<SendConfirmationEmail>));
        Assert.Equal(1, HandledEvents.CountFor<StoreEvent>());
    }

    [Fact]
    public void A_subscribable_only_event_can_be_handled_but_not_dispatched()
    {
        Configure(events => events.AddEventHandler<PartnerPayment, HandlePartnerPayment>());

        // Its handler registers, so this application can react to it.
        Assert.NotNull(Locator.Resolve<HandlePartnerPayment>());

        // And dispatching it is not a failure to catch at runtime: the dispatcher only accepts publishable
        // events, and this one is not, so the attempt does not compile.
        var dispatched = typeof(IEventDispatcher)
            .GetMethod(nameof(IEventDispatcher.DispatchAsync))!
            .GetParameters()[0]
            .ParameterType;

        Assert.Equal(typeof(IPublishableEvent), dispatched);
        Assert.False(typeof(IPublishableEvent).IsAssignableFrom(typeof(PartnerPayment)));
    }

    [Fact]
    public async Task An_event_without_a_declared_name_cannot_be_dispatched()
    {
        Configure(_ => { });

        // Validated before queueing, so the caller still learns about a contract mistake.
        await Assert.ThrowsAsync<MissingEventNameException>(() => DispatchAsync(new UnnamedEvent()));
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
