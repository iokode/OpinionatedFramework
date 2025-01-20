using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Events;
using IOKode.OpinionatedFramework.Jobs;
using IOKode.OpinionatedFramework.Persistence.UnitOfWork;

namespace IOKode.OpinionatedFramework.ContractImplementations.Events;

public class ExecuteHandlerJob(string eventId, Type handlerType) : IJob
{
    public async Task InvokeAsync(CancellationToken cancellationToken)
    {
        var uowFactory = Locator.Resolve<IUnitOfWorkFactory>();
        await using var uow = uowFactory.Create();
        var repository = uow.GetRepository<EventsRepository>();
        var @event = await repository.GetByIdAsync(eventId, cancellationToken);
        await uow.StopTrackingAsync(@event, cancellationToken);

        var handler = Activator.CreateInstance(handlerType)!;
        await (Task) handler.GetType().InvokeMember(nameof(IEventHandler<Event>.HandleAsync), BindingFlags.InvokeMethod, null, handler, [@event, cancellationToken])!;
    }
}

public record ExecuteHandlerJobArguments(string EventId, Type HandlerType) : JobArguments<ExecuteHandlerJob>
{
    public override ExecuteHandlerJob CreateJob() => new(EventId, HandlerType);
}