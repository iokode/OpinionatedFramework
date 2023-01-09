using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Foundation.Notifications;

namespace IOKode.OpinionatedFramework.Contracts;

[Contract]
public interface INotificationDispatcher
{
    public Task DispatchAsync(INotifiable notifiable, Notification notification, CancellationToken cancellationToken);

    public Task DispatchAsync(IEnumerable<INotifiable> notifiableEntities, Notification notification,
        CancellationToken cancellationToken);
}