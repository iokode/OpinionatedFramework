using System;

namespace IOKode.OpinionatedFramework.Persistence.Queries;

public interface IQueryExecutorFactory
{
    /// <summary>
    /// Creates a query executor whose pipeline invokes the given middlewares in order.
    /// </summary>
    /// <remarks>
    /// Middlewares are supplied as types and instantiated once per query execution, so a middleware may hold
    /// per-execution state. Each type must derive from <see cref="QueryMiddleware"/> and expose a public
    /// parameterless constructor; resolve dependencies from the container inside <c>ExecuteAsync</c>.
    /// </remarks>
    /// <param name="middlewareTypes">The middleware types, in invocation order.</param>
    /// <exception cref="ArgumentException">A type does not derive from <see cref="QueryMiddleware"/>.</exception>
    public IQueryExecutor Create(params Type[] middlewareTypes);
}
