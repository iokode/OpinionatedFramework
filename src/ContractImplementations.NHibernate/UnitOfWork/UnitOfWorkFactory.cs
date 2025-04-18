using IOKode.OpinionatedFramework.Persistence.UnitOfWork;
using NHibernate;

namespace IOKode.OpinionatedFramework.ContractImplementations.NHibernate.UnitOfWork;

public class UnitOfWorkFactory(ISessionFactory sessionFactory) : IUnitOfWorkFactory
{
    public IUnitOfWork Create()
    {
        return new UnitOfWork(sessionFactory);
    }
}