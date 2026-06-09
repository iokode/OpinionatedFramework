using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace IOKode.OpinionatedFramework.Persistence.Queries;

/// <summary>
/// Provides high-level query object invocation methods for <see cref="IQueryExecutor"/>.
/// </summary>
public static class QueryExecutorExtensions
{
    /// <summary>
    /// Executes a query object using the provided query executor.
    /// </summary>
    /// <remarks>
    /// Query objects describe the SQL to execute and expose private generated mapping methods named
    /// <c>MapParameters</c> and <c>MapResult</c>. This method uses those mapping methods to translate the query
    /// object into SQL parameters, execute the raw SQL with the proper cardinality, and map the raw rows back to
    /// the public query result type.
    /// </remarks>
    /// <typeparam name="TResult">The public result type returned by the query object.</typeparam>
    /// <param name="queryExecutor">The executor that runs the SQL query.</param>
    /// <param name="query">The query object to execute.</param>
    /// <param name="cancellationToken">A token to cancel the query execution.</param>
    /// <returns>The mapped public query result.</returns>
    public static async Task<TResult> InvokeAsync<TResult>(this IQueryExecutor queryExecutor,
        IQuery<TResult> query,
        CancellationToken cancellationToken)
    {
        var queryType = query.GetType();
        var mapParametersMethod = queryType.GetMethod("MapParameters", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new QueryDefinitionException($"Query type '{queryType.FullName}' does not declare MapParameters.");
        var mapResultMethod = queryType.GetMethod("MapResult", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new QueryDefinitionException($"Query type '{queryType.FullName}' does not declare MapResult.");
        var parameters = mapParametersMethod.Invoke(query, Array.Empty<object?>());
        var rawResultSets = await queryExecutor.QueryResultSetsAsync(query.ResultSets, query.Directives, parameters, null, cancellationToken);
        var mapResultArgument = GetMapResultArgument(mapResultMethod, queryType, rawResultSets);

        return (TResult) mapResultMethod.Invoke(query, new[] { mapResultArgument })!;
    }

    /// <summary>
    /// Gets the raw row type from the generated <c>MapResult</c> method signature.
    /// </summary>
    private static object GetMapResultArgument(MethodInfo mapResultMethod, Type queryType, IReadOnlyList<object> rawResultSets)
    {
        var parameters = mapResultMethod.GetParameters();
        if (parameters.Length != 1)
        {
            throw new QueryDefinitionException($"Query type '{queryType.FullName}' must declare MapResult with a single parameter.");
        }

        var rawResultsType = parameters[0].ParameterType;
        if (rawResultsType == typeof(IReadOnlyList<object>))
        {
            return rawResultSets;
        }

        if (rawResultsType.IsGenericType && rawResultsType.GetGenericTypeDefinition() == typeof(IReadOnlyCollection<>))
        {
            if (rawResultSets.Count != 1)
            {
                throw new QueryDefinitionException($"Query type '{queryType.FullName}' must declare MapResult with IReadOnlyList<object> for multiple result sets.");
            }

            return rawResultSets[0];
        }

        throw new QueryDefinitionException($"Query type '{queryType.FullName}' must declare MapResult with an IReadOnlyCollection<T> or IReadOnlyList<object> parameter.");
    }
}
