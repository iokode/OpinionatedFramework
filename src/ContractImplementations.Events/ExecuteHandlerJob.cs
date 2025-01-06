using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Events;
using IOKode.OpinionatedFramework.Jobs;
using IOKode.OpinionatedFramework.Persistence.UnitOfWork;

namespace IOKode.OpinionatedFramework.ContractImplementations.Events;

public class ExecuteHandlerJob(string eventId, string handlerTypeName) : IJob
{
    public async Task InvokeAsync(CancellationToken cancellationToken)
    {
        var uowFactory = Locator.Resolve<IUnitOfWorkFactory>();
        var uow = uowFactory.Create();
        var repository = uow.GetRepository<EventsRepository>();
        var @event = await repository.GetByIdAsync(eventId, cancellationToken);
        if (@event.DispatchedAt != null)
        {
            return;
        }

        var handlerType = Type.GetType(handlerTypeName)!;
        var handler = Activator.CreateInstance(handlerType, cancellationToken)!;

        await (Task) handler.GetType().InvokeMember("HandleAsync", BindingFlags.InvokeMethod, null, handler, [@event, cancellationToken])!;
        typeof(Event).GetProperty(nameof(Event.DispatchedAt))!.SetValue(@event, DateTimeOffset.UtcNow);

        await uow.SaveChangesAsync(cancellationToken);
    }
}