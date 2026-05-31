using System.Threading;
using System.Threading.Tasks;

namespace IOKode.OpinionatedFramework.Persistence.Queries;

/// <summary>
/// Provides convenience methods for invoking query objects.
/// </summary>
public static class QueryExtensions
{
    /// <summary>
    /// Invokes a query object using the configured <see cref="IQueryExecutor"/>.
    /// </summary>
    /// <typeparam name="TResult">The public result type returned by the query.</typeparam>
    /// <param name="query">The query object to invoke.</param>
    /// <param name="cancellationToken">A token to cancel the query execution.</param>
    /// <returns>The query result.</returns>
    public static async Task<TResult> InvokeAsync<TResult>(
        this IQuery<TResult> query,
        CancellationToken cancellationToken = default)
    {
        var queryExecutor = Locator.Resolve<IQueryExecutor>();
        var result = await queryExecutor.InvokeAsync(query, cancellationToken);
        return result;
    }
}
