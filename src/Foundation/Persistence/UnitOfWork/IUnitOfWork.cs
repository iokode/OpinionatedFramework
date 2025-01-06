using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Ensuring;
using IOKode.OpinionatedFramework.Persistence.QueryBuilder;

namespace IOKode.OpinionatedFramework.Persistence.UnitOfWork;

public interface IUnitOfWork
{
    public bool IsRolledBack { get; }
    
    public Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    
    public Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    
    public Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    
    public bool IsTransactionActive { get; }
    
    public Task AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : Entity;

    public Task<bool> IsTrackedAsync<T>(T entity, CancellationToken cancellationToken = default) where T : Entity;
    
    public Task<TId?> GetEntityIdAsync<TEntity, TId>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : Entity;

    public Task StopTrackingAsync<T>(T entity, CancellationToken cancellationToken = default) where T : Entity;

    public Task<bool> HasChangesAsync(CancellationToken cancellationToken);
    
    /// <summary>
    /// Gets an entity set associated to this unit of work instance.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <returns>The entity set.</returns>
    public IEntitySet<T> GetEntitySet<T>() where T : Entity;

    public Task<ICollection<T>> RawProjection<T>(string query, IList<object>? parameters = null, CancellationToken cancellationToken = default); 

    /// <summary>
    /// Gets a repository instance associated to this unit of work instance.
    /// </summary>
    /// <typeparam name="TRepository">The type of the repository.</typeparam>
    /// <exception cref="UnitOfWorkException"/>
    public TRepository GetRepository<TRepository>() where TRepository : Repository
    {
        var repository = (TRepository)GetRepository(typeof(TRepository));
        return repository;
    }

    /// <summary>
    /// Gets a repository instance associated to this unit of work instance.
    /// </summary>
    /// <param name="repositoryType">The type of the repository.</param>
    /// <exception cref="UnitOfWorkException"/>
    /// <exception cref="ArgumentException">Type is not repository.</exception>
    public Repository GetRepository(Type repositoryType);

    /// <summary>
    /// Persist tracked changes into the persistent storage.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <exception cref="UnitOfWorkException"/>
    public Task SaveChangesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Verifies that the provided type implements <see cref="Repository"/>.
    /// </summary>
    /// <param name="attemptedRepositoryType">The type to check for <see cref="Repository"/> implementation.</param>
    /// <exception cref="ArgumentException">Thrown when the provided '<paramref name="attemptedRepositoryType"/>' does not implement <see cref="Repository"/>.</exception>
    protected static void EnsureTypeIsRepository(Type attemptedRepositoryType)
    {
        Ensure.Type.IsAssignableTo(attemptedRepositoryType, typeof(Repository))
            .ElseThrowsIllegalArgument($"The provided type must be a subtype of {nameof(Repository)}.", nameof(attemptedRepositoryType));
    }
}