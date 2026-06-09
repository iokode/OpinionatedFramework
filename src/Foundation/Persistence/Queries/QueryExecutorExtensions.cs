using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        var mapResultArguments = GetMapResultArguments(mapResultMethod, queryType, query.ResultSets, rawResultSets);

        return (TResult) mapResultMethod.Invoke(query, mapResultArguments)!;
    }

    /// <summary>
    /// Gets the shaped raw result values for the generated <c>MapResult</c> method signature.
    /// </summary>
    private static object?[] GetMapResultArguments(MethodInfo mapResultMethod, Type queryType,
        IReadOnlyList<QueryResultSet> resultSets, IReadOnlyList<object> rawResultSets)
    {
        var parameters = mapResultMethod.GetParameters();
        if (parameters.Length != resultSets.Count)
        {
            throw new QueryDefinitionException($"Query type '{queryType.FullName}' must declare MapResult with one parameter per result set.");
        }

        if (rawResultSets.Count != resultSets.Count)
        {
            throw new QueryDefinitionException($"Query type '{queryType.FullName}' returned an unexpected number of result sets.");
        }

        var arguments = new object?[resultSets.Count];
        for (int i = 0; i < resultSets.Count; i++)
        {
            arguments[i] = GetMapResultArgument(resultSets[i], rawResultSets[i]);
        }

        return arguments;
    }

    private static object? GetMapResultArgument(QueryResultSet resultSet, object rawResultSet)
    {
        return resultSet.Cardinality switch
        {
            QueryCardinality.One => AsEnumerable(rawResultSet).Cast<object>().First(),
            QueryCardinality.ZeroOrOne => AsEnumerable(rawResultSet).Cast<object>().FirstOrDefault(),
            QueryCardinality.ZeroOrMore => rawResultSet,
            _ => throw new QueryDefinitionException($"Unsupported query cardinality: {resultSet.Cardinality}.")
        };
    }

    private static IEnumerable AsEnumerable(object rawResultSet)
    {
        if (rawResultSet is IEnumerable enumerable)
        {
            return enumerable;
        }

        throw new QueryDefinitionException("Raw result set must be enumerable.");
    }
}
