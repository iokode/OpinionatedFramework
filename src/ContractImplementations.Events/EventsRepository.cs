using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Events;
using IOKode.OpinionatedFramework.Persistence.UnitOfWork;

namespace IOKode.OpinionatedFramework.ContractImplementations.Events;

public class EventsRepository : Repository
{
    public async Task<Event> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await GetEntitySet<Event>().GetByIdAsync(id, cancellationToken);
    }

    public async Task AddAsync(Event @event, CancellationToken cancellationToken = default)
    {
        await UnitOfWork.AddAsync(@event, cancellationToken);
    }
}