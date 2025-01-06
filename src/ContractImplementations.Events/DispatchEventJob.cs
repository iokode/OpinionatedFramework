using System;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Events;
using IOKode.OpinionatedFramework.Jobs;
using IOKode.OpinionatedFramework.Jobs.Extensions;

namespace IOKode.OpinionatedFramework.ContractImplementations.Events;

public class DispatchEventJob(string eventId, string eventTypeName) : IJob
{
    public async Task InvokeAsync(CancellationToken cancellationToken)
    {
        var eventType = Type.GetType(eventTypeName)!;
        var handlers = EventHandlers.GetHandlerTypes(eventType);

        foreach (var handler in handlers)
        {
            var handlerJob = new ExecuteHandlerJob(eventId, handler.AssemblyQualifiedName!);
            await handlerJob.EnqueueAsync(Queue.Create("Events"), cancellationToken);
        }
    }
}