using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Persistence.UnitOfWork;
using Filter = IOKode.OpinionatedFramework.Persistence.QueryBuilder.Filters.Filter;

namespace IOKode.OpinionatedFramework.Persistence.QueryBuilder;

public interface IEntitySet<T> where T : Entity
{
    Task<T> SingleAsync(Filter filter, CancellationToken cancellationToken = default);
    Task<T?> SingleOrDefaultAsync(Filter filter, CancellationToken cancellationToken = default);
    Task<T> FirstAsync(Filter filter, CancellationToken cancellationToken = default);
    Task<T?> FirstOrDefaultAsync(Filter filter, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<T>> ManyAsync(Filter filter, CancellationToken cancellationToken = default);
}