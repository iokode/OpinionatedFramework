using System.Collections.Generic;
using IOKode.OpinionatedFramework.Persistence.Queries;

namespace IOKode.OpinionatedFramework.ContractImplementations.NHibernate.QueryExecutor;

public class QueryExecutorOptions
{
    public IQueryExecutorConfiguration? QueryExecutorConfiguration { get; set; }
    public List<QueryMiddleware> Middlewares { get; set; } = [];
}