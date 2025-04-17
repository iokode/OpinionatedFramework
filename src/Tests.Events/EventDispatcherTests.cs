using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.ContractImplementations.Events;
using IOKode.OpinionatedFramework.Events;
using IOKode.OpinionatedFramework.Jobs;
using IOKode.OpinionatedFramework.Persistence.UnitOfWork;
using IOKode.OpinionatedFramework.Tests.Events.Config;
using IOKode.OpinionatedFramework.Tests.Helpers;
using IOKode.OpinionatedFramework.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace IOKode.OpinionatedFramework.Tests.Events;

public class EventDispatcherTests(ITestOutputHelper output, EventsTestsFixture fixture) : EventsTestsBase(fixture)
{
    [Fact]
    public async Task DispatchEvent()
    {
        // Arrange
        EventHandlers.Register<Event1, EventHandler1>();
        EventHandlers.Register<Event1, EventHandler2>();

        var enqueuer = Locator.Resolve<IJobEnqueuer>();
        var uowFactory = Locator.Resolve<IUnitOfWorkFactory>();
        var dispatcher = new EventDispatcher(uowFactory, enqueuer, new ConfigurationProvider(new Dictionary<string, object>
        {
            {"Events:QueueName", "eventing"}
        }));
        var @event = new Event1(3, "test");

        // Act
        output.WriteLine("Pre Act");
        await dispatcher.DispatchAsync(@event, default);
        await PollingUtility.WaitUntilTrueAsync(() => EventHandler1.IsExecuted && EventHandler2.IsExecuted, 30_000, 1000);
        await using var uow = uowFactory.Create();
        var events = await uow.GetEntitySet<Event>().ManyAsync();
        var eventDispatched = events.OfType<Event1>().Single();

        // Assert
        output.WriteLine("Pre assert");
        Assert.Equal(3, eventDispatched.Prop1);
        Assert.Equal("test", eventDispatched.Prop2);
        Assert.NotNull(eventDispatched.DispatchedAt);
        Assert.True(EventHandler1.IsExecuted);
        Assert.True(EventHandler2.IsExecuted);
    }
}

public class EventHandler1 : IEventHandler<Event1>
{
    public static bool IsExecuted;

    public Task HandleAsync(Event1 @event, CancellationToken cancellationToken)
    {
        IsExecuted = true;
        return Task.CompletedTask;
    }
}

public class EventHandler2 : IEventHandler<Event1>
{
    public static bool IsExecuted;

    public Task HandleAsync(Event1 @event, CancellationToken cancellationToken)
    {
        IsExecuted = true;
        return Task.CompletedTask;
    }
}