using System;
using System.Collections.Generic;
using IOKode.OpinionatedFramework.Persistence.Queries;

namespace IOKode.OpinionatedFramework.ContractImplementations.NHibernate.QueryExecutor;

public class QueryExecutorOptions
{
    internal readonly List<Type> middlewareTypes = new();

    public IQueryExecutorConfiguration? QueryExecutorConfiguration { get; set; }

    /// <summary>
    /// Adds a middleware to the query pipeline.
    /// </summary>
    /// <remarks>
    /// Middlewares are invoked in the order they were added, and one instance is created per query execution.
    /// <typeparamref name="TMiddleware"/> must expose a public parameterless constructor; resolve dependencies
    /// from the container inside <c>ExecuteAsync</c>.
    /// </remarks>
    /// <typeparam name="TMiddleware">The middleware to invoke.</typeparam>
    public void AddMiddleware<TMiddleware>() where TMiddleware : QueryMiddleware
    {
        this.middlewareTypes.Add(typeof(TMiddleware));
    }
}
