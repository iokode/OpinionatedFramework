using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.ContractImplementations.Events;
using IOKode.OpinionatedFramework.Events;
using IOKode.OpinionatedFramework.Jobs;
using IOKode.OpinionatedFramework.Persistence.UnitOfWork;
using Xunit;
using Xunit.Abstractions;

namespace IOKode.OpinionatedFramework.Tests.Events;

public class EventDispatcherTests(ITestOutputHelper output) : EventsTestsBase(output)
{
    public static bool isEvent1Executed = false;
    
    [Fact]
    public async Task DispatchEvent()
    {
        // Arrange
        EventHandlers.Register<Event1, EventHandler1>();
        await CreateEventsTableQueryAsync();

        var enqueuer = Locator.Resolve<IJobEnqueuer>();
        var uowFactory = Locator.Resolve<IUnitOfWorkFactory>();
        var dispatcher = new EventDispatcher(uowFactory, enqueuer);
        var @event = new Event1(3, "test");

        // Act
        await dispatcher.DispatchAsync(@event, default);
        await Task.Delay(20000);
        var events = await uowFactory.Create().GetEntitySet<Event>().ManyAsync();
        var eventDispatched = events.OfType<Event1>().Single();

        // Assert
        Assert.True(isEvent1Executed);
        Assert.Equal(3, eventDispatched.Prop1);
        Assert.Equal("test", eventDispatched.Prop2);
        Assert.NotNull(eventDispatched.DispatchedAt);
        
        // Post assert
        await DropEventsTableQueryAsync();
    }
    
    public class EventHandler1 : IEventHandler<Event1>
    {
        public Task HandleAsync(Event1 @event, CancellationToken cancellationToken)
        {
            isEvent1Executed = true;
            return Task.CompletedTask;
        }
    }
}