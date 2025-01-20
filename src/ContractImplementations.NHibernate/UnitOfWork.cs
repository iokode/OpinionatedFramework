using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using IOKode.OpinionatedFramework.Persistence.QueryBuilder;
using IOKode.OpinionatedFramework.Persistence.UnitOfWork;
using IOKode.OpinionatedFramework.Persistence.UnitOfWork.Exceptions;
using NHibernate;

namespace IOKode.OpinionatedFramework.ContractImplementations.NHibernate;

public class UnitOfWork : IUnitOfWork
{
    private readonly Dictionary<Type, Repository> repositories = new();
    private readonly ISession session;
    private ITransaction? transaction;
    private bool isRollbacked;

    public UnitOfWork(ISessionFactory sessionFactory)
    {
        this.session = sessionFactory.OpenSession();
    }

    public bool IsRolledBack => this.isRollbacked;

    public Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        ThrowsIfRolledBack();
        
        this.transaction = this.session.BeginTransaction();
        return Task.CompletedTask;
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        ThrowsIfRolledBack();

        if (this.transaction is null)
        {
            throw new InvalidOperationException("No transaction is active.");
        }

        await this.transaction.CommitAsync(cancellationToken);
        this.transaction.Dispose();
        this.transaction = null;
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        ThrowsIfRolledBack();

        if (this.transaction is null)
        {
            throw new InvalidOperationException("No transaction is active.");
        }
        
        await this.transaction.RollbackAsync(cancellationToken);
        this.transaction.Dispose();
        this.transaction = null;
        this.session.Clear();
        await DisposeAsync();
        this.isRollbacked = true;
    }

    public bool IsTransactionActive
    {
        get
        {
            ThrowsIfRolledBack();
            return this.transaction is {IsActive: true};
        }
    }

    public async Task AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : Entity
    {
        ThrowsIfRolledBack();
        await this.session.PersistAsync(entity, cancellationToken);
    }

    public Task<bool> IsTrackedAsync<T>(T entity, CancellationToken cancellationToken = default) where T : Entity
    {
        ThrowsIfRolledBack();
        return Task.FromResult(session.Contains(entity));
    }

    public Task<TId?> GetEntityIdAsync<TEntity, TId>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : Entity
    {
        // todo catch exceptions and create own exceptions
        return Task.FromResult((TId?)this.session.GetIdentifier(entity));
    }

    public async Task StopTrackingAsync<T>(T entity, CancellationToken cancellationToken = default) where T : Entity
    {
        ThrowsIfRolledBack();
        await this.session.EvictAsync(entity, cancellationToken);
    }

    public async Task<bool> HasChangesAsync(CancellationToken cancellationToken)
    {
        ThrowsIfRolledBack();
        return await this.session.IsDirtyAsync(cancellationToken);
    }

    public IEntitySet<T> GetEntitySet<T>() where T : Entity
    {
        ThrowsIfRolledBack();
        return new EntitySet<T>(this.session);
    }

    public async Task<ICollection<T>> RawProjection<T>(string query, object? parameters = null, CancellationToken cancellationToken = default)
    {
        ThrowsIfRolledBack();

        var transaction = GetTransaction();
        return (await this.session.Connection.QueryAsync<T>(query, parameters, transaction)).ToArray();
    }

    public Repository GetRepository(Type repositoryType)
    {
        ThrowsIfRolledBack();
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

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        ThrowsIfRolledBack();
        bool isTransaction = IsTransactionActive;

        if (!isTransaction)
        {
            await BeginTransactionAsync(cancellationToken);
        }
        
        await this.session.FlushAsync(cancellationToken);

        if (!isTransaction)
        {
            await CommitTransactionAsync(cancellationToken);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (this.IsTransactionActive)
        {
            await this.transaction!.RollbackAsync();
        }
        this.transaction?.Dispose();
        this.session.Dispose();
    }

    private void ThrowsIfRolledBack()
    {
        if (IsRolledBack)
        {
            throw new UnitOfWorkRolledBackException();
        }
    }
    
    private IDbTransaction GetTransaction()
    {
        using(var command = this.session.Connection.CreateCommand())
        {
            this.session.Transaction.Enlist(command);
            return command.Transaction;
        }
    }
}