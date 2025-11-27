using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using IOKode.OpinionatedFramework.Persistence.Queries;

namespace IOKode.OpinionatedFramework.ContractImplementations.NHibernate.QueryExecutor;

public class NHibernateQueryExecutionExecutorContext : IQueryExecutionContext
{
    public required IDbTransaction? Transaction { get; set; }
    public required bool IsExecuted { get; set; }
    public required ICollection<string> Directives { get; set; }
    public required object? Parameters { get; set; }
    public required CancellationToken CancellationToken { get; set; }
    public required string RawQuery { get; set; }
    public required IReadOnlyList<object> Results { get; set; }
    public required Guid TraceID { get; set; }
}