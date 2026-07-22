using System;
using System.Linq;
using IOKode.OpinionatedFramework.Persistence.Queries;
using NHibernate;

namespace IOKode.OpinionatedFramework.ContractImplementations.NHibernate.QueryExecutor;

public class QueryExecutorFactory(ISessionFactory sessionFactory, IQueryExecutorConfiguration configuration) : IQueryExecutorFactory
{
    public IQueryExecutor Create(params Type[] middlewareTypes)
    {
        ArgumentNullException.ThrowIfNull(middlewareTypes);

        var invalidType = middlewareTypes.FirstOrDefault(type => !typeof(QueryMiddleware).IsAssignableFrom(type));
        if (invalidType is not null)
        {
            throw new ArgumentException(
                $"Type '{invalidType.FullName}' does not derive from {nameof(QueryMiddleware)}.",
                nameof(middlewareTypes));
        }

        return new QueryExecutor(sessionFactory, configuration, middlewareTypes);
    }
}