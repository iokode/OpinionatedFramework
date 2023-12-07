using System;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Ensuring;

namespace IOKode.OpinionatedFramework.Persistence;

public interface IUnitOfWork
{
    /// <summary>
    /// Gets a repository instance associated to this unit of work instance.
    /// </summary>
    /// <typeparam name="TRepository">The type of the repository.</typeparam>
    /// <exception cref="UnitOfWorkException"/>
    public TRepository GetRepository<TRepository>() where TRepository : IRepository
    {
        var repository = (TRepository) GetRepository(typeof(TRepository));
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

    /// <summary>
    /// Verifies that the provided type implements <see cref="IRepository"/>.
    /// </summary>
    /// <param name="attemptedRepositoryType">The type to check for <see cref="IRepository"/> implementation.</param>
    /// <exception cref="ArgumentException">Thrown when the provided '<paramref name="attemptedRepositoryType"/>' does not implement <see cref="IRepository"/>.</exception>
    protected void EnsureTypeIsRepository(Type attemptedRepositoryType)
    {
        Ensure.Type.IsAssignableTo(attemptedRepositoryType, typeof(IRepository))
            .ElseThrowsIllegalArgument($"The provided type must be a type that implements {nameof(IRepository)}.",
                nameof(attemptedRepositoryType));
    }
}