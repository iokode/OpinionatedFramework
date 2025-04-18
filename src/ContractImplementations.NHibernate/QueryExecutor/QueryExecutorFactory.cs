using IOKode.OpinionatedFramework.Persistence.Queries;
using NHibernate;

namespace IOKode.OpinionatedFramework.ContractImplementations.NHibernate.QueryExecutor;

public class QueryExecutorFactory(ISessionFactory sessionFactory, IQueryExecutorConfiguration configuration) : IQueryExecutorFactory
{
    public IQueryExecutor Create(params QueryMiddleware[] middlewares)
    {
        return new QueryExecutor(sessionFactory, configuration, middlewares);
    }
}