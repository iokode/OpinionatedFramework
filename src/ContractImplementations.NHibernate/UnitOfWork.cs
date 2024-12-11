using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Persistence.QueryBuilder;
using IOKode.OpinionatedFramework.Persistence.UnitOfWork;
using NHibernate;

namespace IOKode.OpinionatedFramework.ContractImplementations.NHibernate;

public class UnitOfWork : IUnitOfWork, IAsyncDisposable
{
    private readonly Dictionary<Type, Repository> repositories = new();
    private readonly ISession session;
    private ITransaction? transaction;
    private readonly ISessionFactory sessionFactory;

    public UnitOfWork(ISessionFactory sessionFactory)
    {
        this.session = sessionFactory.OpenSession();
        this.sessionFactory = sessionFactory;
    }

    public Task BeginTransactionAsync()
    {
        this.transaction = this.session.BeginTransaction();
        return Task.CompletedTask;
    }

    public async Task CommitTransactionAsync()
    {
        if (this.transaction is null)
        {
            throw new InvalidOperationException("No transaction is active.");
        }

        await this.transaction.CommitAsync();
    }

    public async Task RollbackTransactionAsync()
    {
        if (this.transaction is null)
        {
            throw new InvalidOperationException("No transaction is active.");
        }

        await this.transaction.RollbackAsync();
    }

    public bool IsTransactionActive => this.transaction is { IsActive: true };

    public async Task AddAsync<T>(T entity, CancellationToken cancellationToken = default) where T : Entity
    {
        await this.session.PersistAsync(entity, cancellationToken);
    }

    public IEntitySet<T> GetEntitySet<T>() where T : Entity
    {
        return new EntitySet<T>(this.session);
    }

    public IEntitySet<T> GetUntrackedEntitySet<T>() where T : Entity
    {
        // todo
        throw new NotImplementedException();
    }

    public Repository GetRepository(Type repositoryType)
    {
        IUnitOfWork.EnsureTypeIsRepository(repositoryType);

        if (this.repositories.TryGetValue(repositoryType, out var repo))
        {
            return repo;
        }

        // Create an instance of the repository. This assumes the repository has
        // a parameterless constructor or a constructor we can access non-publicly.
        repo = (Repository)Activator.CreateInstance(repositoryType, nonPublic: true)!;

        // Use reflection to set the UnitOfWork property.
        // The property is defined on the Repository base class, so we reflect on typeof(Repository).
        // The compiler generates a backing field named `<UnitOfWork>k__BackingField` for auto-properties.
        var field = typeof(Repository).GetField("unitOfWork", BindingFlags.Instance | BindingFlags.NonPublic);

        if (field is null)
        {
            throw new UnreachableException("Could not find the 'unitOfWork' field.");
        }

        // Set the field value to the current IUnitOfWork instance
        field.SetValue(repo, this);

        this.repositories[repositoryType] = repo;
        return repo;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return this.session.FlushAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (IsTransactionActive)
        {
            await this.transaction!.RollbackAsync();
        }
        this.transaction?.Dispose();
        this.session.Dispose();
    }
}