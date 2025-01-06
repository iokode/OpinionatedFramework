using System;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Events;
using IOKode.OpinionatedFramework.Jobs;
using IOKode.OpinionatedFramework.Persistence.UnitOfWork;

namespace IOKode.OpinionatedFramework.ContractImplementations.Events;

public class EventDispatcher(IUnitOfWorkFactory uowFactory, IJobEnqueuer jobEnqueuer) : IEventDispatcher
{
    public async Task DispatchAsync(Event @event, CancellationToken cancellationToken)
    {
        if (@event.DispatchedAt != null)
        {
            return;
        }

        var uow = uowFactory.Create();
        var repository = uow.GetRepository<EventsRepository>();
        await repository.AddAsync(@event, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);
        string id = (await uow.GetEntityIdAsync<Event, string>(@event, cancellationToken))!;
        var eventTypeName = @event.GetType().AssemblyQualifiedName!;

        var job = new DispatchEventJob(id, eventTypeName);
        await jobEnqueuer.EnqueueAsync(job, cancellationToken);
    }
}