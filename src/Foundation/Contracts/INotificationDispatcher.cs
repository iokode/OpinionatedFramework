using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Facades;
using IOKode.OpinionatedFramework.Notifications;
using Notification = IOKode.OpinionatedFramework.Notifications.Notification;

namespace IOKode.OpinionatedFramework.Contracts;

[AddToFacade("Notification")]
public interface INotificationDispatcher
{
    public Task DispatchAsync(INotifiable notifiable, Notification notification, CancellationToken cancellationToken);

    public Task DispatchAsync(IEnumerable<INotifiable> notifiableEntities, Notification notification,
        CancellationToken cancellationToken);
}