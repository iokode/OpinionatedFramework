using System;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Ensuring;

namespace IOKode.OpinionatedFramework.Contracts.Persistence;

public interface IUnitOfWork
{
    /// <summary>
    /// Gets a repository instance associated to this unit of work instance.
    /// </summary>
    /// <typeparam name="TRepository">The type of the repository.</typeparam>
    /// <exception cref="UnitOfWorkException"/>
    public TRepository GetRepository<TRepository>() where TRepository : IRepository
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
    public IRepository GetRepository(Type repositoryType);

    /// <summary>
    /// Persist tracked changes into the persistent storage.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <exception cref="UnitOfWorkException"/>
    public Task SaveChangesAsync(CancellationToken cancellationToken);

    protected void EnsureTypeIsRepository(Type attemptedRepositoryType)
    {
        Ensure.Argument().Type.IsAssignableTo(attemptedRepositoryType, typeof(IRepository));
    }
}