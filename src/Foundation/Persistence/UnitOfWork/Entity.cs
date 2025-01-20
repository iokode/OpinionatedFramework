namespace IOKode.OpinionatedFramework.Persistence.UnitOfWork;

public abstract class Entity;

public abstract class Entity<TId> : Entity
{
    public TId? Id { get; protected set; }
}