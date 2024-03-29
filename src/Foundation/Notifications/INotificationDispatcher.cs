using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Facades;

namespace IOKode.OpinionatedFramework.Notifications;

[AddToFacade("Notification")]
public interface INotificationDispatcher
{
    public Task DispatchAsync(INotifiable notifiable, Notification notification, CancellationToken cancellationToken);

    public Task DispatchAsync(IEnumerable<INotifiable> notifiableEntities, Notification notification,
        CancellationToken cancellationToken);
}