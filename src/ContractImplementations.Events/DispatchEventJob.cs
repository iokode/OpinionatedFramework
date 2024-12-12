using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Events;
using IOKode.OpinionatedFramework.Facades;
using IOKode.OpinionatedFramework.Jobs;

namespace IOKode.OpinionatedFramework.ContractImplementations.Events;

// todo el constructor no deberia recibir el evento por parametro, deberi recibir idealmente primitivos
public class DispatchEventJob(Event @event) : IJob
{
    public Task InvokeAsync(CancellationToken cancellationToken)
    {
        var eventType = @event.GetType();
        var handlers = Enumerable.Empty<IEventHandler<Event>>();

        foreach (var handler in handlers)
        {
            var handlerJob = new ExecuteHandlerJob();
            Job.EnqueueAsync(handlerJob, cancellationToken);
        }

        return Task.CompletedTask;
    }
}