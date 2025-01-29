using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Filter = IOKode.OpinionatedFramework.Persistence.UnitOfWork.QueryBuilder.Filters.Filter;

namespace IOKode.OpinionatedFramework.Persistence.UnitOfWork.QueryBuilder;

public interface IEntitySet<T> where T : Entity
{
    Task<T> GetByIdAsync(object id, CancellationToken cancellationToken = default);
    Task<T> GetByIdOrDefaultAsync(object id, CancellationToken cancellationToken = default);
    Task<T> SingleAsync(Filter? filter = null, CancellationToken cancellationToken = default);
    Task<T?> SingleOrDefaultAsync(Filter? filter = null, CancellationToken cancellationToken = default);
    Task<T> FirstAsync(Filter? filter = null, CancellationToken cancellationToken = default);
    Task<T?> FirstOrDefaultAsync(Filter? filter = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<T>> ManyAsync(Filter? filter = null, CancellationToken cancellationToken = default);
}