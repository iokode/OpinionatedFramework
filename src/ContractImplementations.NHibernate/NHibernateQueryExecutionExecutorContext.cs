using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using IOKode.OpinionatedFramework.Persistence.Queries;

namespace IOKode.OpinionatedFramework.ContractImplementations.NHibernate;

public class NHibernateQueryExecutionExecutorContext : IQueryExecutionContext
{
    public IDbTransaction? Transaction { get; set; }
    public bool IsExecuted { get; set; }
    public ICollection<string> Directives { get; set; }
    public object? Parameters { get; set; }
    public CancellationToken CancellationToken { get; set; }
    public string RawQuery { get; set; }
    public IReadOnlyList<object> Results { get; set; }
    public Guid TraceID { get; set; }
}