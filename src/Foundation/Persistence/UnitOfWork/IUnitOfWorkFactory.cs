using IOKode.OpinionatedFramework.Facades;

namespace IOKode.OpinionatedFramework.Persistence.UnitOfWork;

[AddToFacade("Uow")]
public interface IUnitOfWorkFactory
{
    public IUnitOfWork Create();
}