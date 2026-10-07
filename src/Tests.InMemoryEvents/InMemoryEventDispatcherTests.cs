using System;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.ContractImplementations.InMemoryEvents;
using IOKode.OpinionatedFramework.Events;
using IOKode.OpinionatedFramework.Events.Exceptions;
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
        Configure(events =>
        {
            events.Publishes<OrderSubmitted>();
            events.Handles<OrderSubmitted, SlowHandler>();
        });

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
            // Declared in both directions, because this application both raises it and reacts to it.
            events.Publishes<OrderSubmitted>();
            events.Handles<OrderSubmitted, SendConfirmationEmail>();
            events.Handles<OrderSubmitted, UpdateStatistics>();
        });

        await DispatchAsync(new OrderSubmitted {OrderId = Guid.NewGuid(), Customer = "ada"});

        Assert.True(await WaitForAsync(() => HandledEvents.WasHandledBy<SendConfirmationEmail>()
                                             && HandledEvents.WasHandledBy<UpdateStatistics>()));
    }

    [Fact]
    public async Task A_handler_written_against_the_event_interface_is_declared_once_per_event_it_covers()
    {
        Configure(events =>
        {
            events.Publishes<OrderSubmitted>();
            events.Publishes<OrderCancelled>();
            events.Handles<OrderSubmitted, StoreEvent>();
            events.Handles<OrderCancelled, StoreEvent>();
        });

        await DispatchAsync(new OrderSubmitted {OrderId = Guid.NewGuid(), Customer = "ada"});
        await DispatchAsync(new OrderCancelled {OrderId = Guid.NewGuid()});

        Assert.True(await WaitForAsync(() => HandledEvents.CountFor<StoreEvent>() == 2));
    }

    [Fact]
    public async Task A_handler_written_against_the_event_interface_receives_the_concrete_event_with_its_data()
    {
        Configure(events =>
        {
            events.Publishes<OrderSubmitted>();
            events.Handles<OrderSubmitted, StoreEvent>();
        });

        await DispatchAsync(new OrderSubmitted {OrderId = Guid.NewGuid(), Customer = "ada"});

        // The handler takes ISubscribableEvent, and the contravariance is what let it be declared for the
        // concrete event. What it receives is that event, with the members only it declares.
        Assert.True(await WaitForAsync(
            () => HandledEvents.SawPayload<StoreEvent>("tests.order-submitted", "ada")));
    }

    [Fact]
    public async Task A_handler_is_retried_as_its_policy_declares()
    {
        Configure(events =>
        {
            events.Publishes<OrderSubmitted>();
            events.Handles<OrderSubmitted, FailOnFirstAttempt>(policy => policy.Retry(1));
        });

        await DispatchAsync(new OrderSubmitted {OrderId = Guid.NewGuid(), Customer = "ada"});

        Assert.True(await WaitForAsync(HandledEvents.WasHandledBy<FailOnFirstAttempt>));
        Assert.Equal(2, HandledEvents.Attempts);
    }

    [Fact]
    public async Task A_failing_handler_does_not_stop_the_others()
    {
        Configure(events =>
        {
            events.Publishes<OrderCancelled>();
            events.Handles<OrderCancelled, AlwaysFail>();
            events.Handles<OrderCancelled, RecordCancellation>();
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
            events.Publishes<OrderSubmitted>();
            events.Handles<OrderSubmitted, ConcurrencyProbe>();
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
            events.Publishes<AuditRecorded>();
            events.Publishes<OrderSubmitted>();
            events.Handles<OrderSubmitted, SendConfirmationEmail>();
            events.Handles<OrderSubmitted, StoreEvent>();
        });

        // It is declared as raised and nothing else: no handler can be declared for it, because reacting to an
        // event the application is not meant to react to does not compile.
        await DispatchAsync(new AuditRecorded {OrderId = Guid.NewGuid()});
        await DispatchAsync(new OrderSubmitted {OrderId = Guid.NewGuid(), Customer = "ada"});

        // Waiting for the symmetric event proves the queue was drained past the publishable-only one.
        Assert.True(await WaitForAsync(HandledEvents.WasHandledBy<SendConfirmationEmail>));
        Assert.Equal(1, HandledEvents.CountFor<StoreEvent>());
    }

    [Fact]
    public void A_subscribable_only_event_can_be_handled_but_not_dispatched()
    {
        Configure(events => events.Handles<PartnerPayment, HandlePartnerPayment>());

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
    public async Task An_event_the_application_did_not_declare_it_raises_cannot_be_dispatched()
    {
        Configure(events =>
        {
            events.Publishes<OrderSubmitted>();
            events.Handles<OrderSubmitted, SendConfirmationEmail>();
        });

        var exception = await Assert.ThrowsAsync<MissingPublishDeclarationException>(
            () => DispatchAsync(new AuditRecorded {OrderId = Guid.NewGuid()}));

        // The failure names the declaration that is missing, so what to write is not left to be worked out.
        Assert.Equal(typeof(AuditRecorded), exception.EventType);
        Assert.Contains("events.Publishes<AuditRecorded>()", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void An_event_without_a_declared_name_cannot_be_declared()
    {
        // Caught where the event is declared, which is the earliest point at which the name is asked for.
        Assert.Throws<MissingEventNameException>(() => Configure(events => events.Publishes<UnnamedEvent>()));
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
