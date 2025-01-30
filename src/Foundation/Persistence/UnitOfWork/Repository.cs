using IOKode.OpinionatedFramework.Persistence.UnitOfWork.QueryBuilder;

namespace IOKode.OpinionatedFramework.Persistence.UnitOfWork;

public abstract class Repository
{
    private IUnitOfWork unitOfWork = null!;
    
    /// <summary>
    ///     The unit of work that created this repository.
    /// </summary>
    /// <remarks>
    ///     This is set by the contract implementation when the repository is created.
    /// </remarks>
    protected IUnitOfWork UnitOfWork => this.unitOfWork;
    
    protected IEntitySet<T> GetEntitySet<T>() where T : Entity
    {
        return UnitOfWork.GetEntitySet<T>();
    }
}