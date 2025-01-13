using System;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Configuration;
using IOKode.OpinionatedFramework.Events;
using IOKode.OpinionatedFramework.Jobs;
using IOKode.OpinionatedFramework.Persistence.UnitOfWork;

namespace IOKode.OpinionatedFramework.ContractImplementations.Events;

public class EventDispatcher(IUnitOfWorkFactory uowFactory, IJobEnqueuer jobEnqueuer, IConfigurationProvider config) : IEventDispatcher
{
    public async Task DispatchAsync(Event @event, CancellationToken cancellationToken)
    {
        var queue = Queue.Create(config.GetValue<string>("Events:QueueName") ?? "events");
        
        if (@event.IsDispatched)
        {
            return;
        }
        
        typeof(Event).GetProperty(nameof(Event.DispatchedAt))!.SetValue(@event, DateTime.UtcNow);
        string id;
        
        await using (var uow = uowFactory.Create())
        {
            var repository = uow.GetRepository<EventsRepository>();
            await repository.AddAsync(@event, cancellationToken);
            await uow.SaveChangesAsync(cancellationToken);
            id = (await uow.GetEntityIdAsync<Event, string>(@event, cancellationToken))!;
        }

        var handlerTypes = EventHandlers.GetHandlerTypes(@event.GetType());
        foreach (var handlerType in handlerTypes)
        {
            var handlerJob = new ExecuteHandlerJob(id, handlerType);
            await jobEnqueuer.EnqueueAsync(queue, handlerJob, cancellationToken);
        }
    }
}