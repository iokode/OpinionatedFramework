using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Jobs;
using IOKode.OpinionatedFramework.Persistence.UnitOfWork;

namespace IOKode.OpinionatedFramework.ContractImplementations.Events;

public class ExecuteHandlerJob(string eventId, Type handlerType) : IJob
{
    public string EventId { get; } = eventId;
    public Type HandlerType { get; } = handlerType;

    public async Task InvokeAsync(CancellationToken cancellationToken)
    {
        var uowFactory = Locator.Resolve<IUnitOfWorkFactory>();
        await using var uow = uowFactory.Create();
        var repository = uow.GetRepository<EventsRepository>();
        var @event = await repository.GetByIdAsync(EventId, cancellationToken);
        await uow.StopTrackingAsync(@event, cancellationToken);

        var handler = Activator.CreateInstance(HandlerType)!;
        await (Task) handler.GetType().InvokeMember("HandleAsync", BindingFlags.InvokeMethod, null, handler, [@event, cancellationToken])!;
    }
}