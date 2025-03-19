using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Events;
using IOKode.OpinionatedFramework.Jobs;
using IOKode.OpinionatedFramework.Persistence.UnitOfWork;

namespace IOKode.OpinionatedFramework.ContractImplementations.Events;

public class ExecuteHandlerJob(Guid eventId, Type handlerType) : Job
{
    public override async Task ExecuteAsync(IJobExecutionContext context)
    {
        var uowFactory = Locator.Resolve<IUnitOfWorkFactory>();
        await using var uow = uowFactory.Create();
        var repository = uow.GetRepository<EventsRepository>();
        var @event = await repository.GetByIdAsync(eventId, context.CancellationToken);
        await uow.StopTrackingAsync(@event, context.CancellationToken);

        var handler = Activator.CreateInstance(handlerType)!;
        await (Task) handler.GetType().InvokeMember(nameof(IEventHandler<Event>.HandleAsync), BindingFlags.InvokeMethod, null, handler, [@event, context.CancellationToken])!;
    }
}

public class ExecuteHandlerJobCreator(Guid EventId, Type HandlerType) : JobCreator<ExecuteHandlerJob>
{
    public override ExecuteHandlerJob CreateJob() => new(EventId, HandlerType);
    public override string GetJobName() => $"Execute handler for event {EventId}.";
}