using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using IOKode.OpinionatedFramework.Persistence.Queries;

namespace IOKode.OpinionatedFramework.ContractImplementations.NHibernate.QueryExecutor;

public class NHibernateQueryExecutionExecutorContext : IQueryExecutionContext
{
    private IReadOnlyList<object> results = Array.Empty<object>();

    public required IDbTransaction? Transaction { get; set; }
    public required bool IsExecuted { get; set; }
    public required bool HasMultipleResultSets { get; set; }
    public required ICollection<string> Directives { get; set; }
    public required object? Parameters { get; set; }
    public required CancellationToken CancellationToken { get; set; }
    public required string RawQuery { get; init; }
    public required IReadOnlyList<IQueryResultSetExecutionContext> ResultSets { get; init; }
    public required IReadOnlyList<object> Results
    {
        get
        {
            if (!IsExecuted)
            {
                throw new InvalidOperationException("The query has not been executed yet.");
            }

            return results;
        }
        set => results = value;
    }
    public required Guid TraceID { get; set; }
}
