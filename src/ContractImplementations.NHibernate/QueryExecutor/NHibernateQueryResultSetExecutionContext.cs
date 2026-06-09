using System;
using System.Collections.Generic;
using IOKode.OpinionatedFramework.Persistence.Queries;

namespace IOKode.OpinionatedFramework.ContractImplementations.NHibernate.QueryExecutor;

public class NHibernateQueryResultSetExecutionContext : IQueryResultSetExecutionContext
{
    public required string? Name { get; init; }
    public required IReadOnlyList<string> Directives { get; init; }
    public required QueryCardinality Cardinality { get; init; }
    public required QueryResultShape Shape { get; init; }
    public required Type ResultType { get; init; }
    public required string? ScalarColumnName { get; init; }
    public required string RawQuery { get; set; }
    public IReadOnlyList<object> Results { get; set; } = Array.Empty<object>();
}
