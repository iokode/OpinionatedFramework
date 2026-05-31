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
            ?? throw new InvalidOperationException($"Query type '{queryType.FullName}' does not declare MapParameters.");
        var mapResultMethod = queryType.GetMethod("MapResult", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"Query type '{queryType.FullName}' does not declare MapResult.");
        var rowType = GetRowType(mapResultMethod, queryType);
        var parameters = mapParametersMethod.Invoke(query, Array.Empty<object?>());
        var cardinality = query.Cardinality;
        var invokeTypedMethod = typeof(QueryExecutorExtensions)
            .GetMethod(nameof(InvokeTypedAsync), BindingFlags.Static | BindingFlags.NonPublic)!
            .MakeGenericMethod(typeof(TResult), rowType);
        var resultTask = (Task<TResult>) invokeTypedMethod.Invoke(null,
            new object?[] { queryExecutor, query, parameters, mapResultMethod, cardinality, cancellationToken })!;

        return await resultTask;
    }

    /// <summary>
    /// Gets the raw row type from the generated <c>MapResult</c> method signature.
    /// </summary>
    private static Type GetRowType(MethodInfo mapResultMethod, Type queryType)
    {
        var parameters = mapResultMethod.GetParameters();
        if (parameters.Length != 1)
        {
            throw new InvalidOperationException($"Query type '{queryType.FullName}' must declare MapResult with a single parameter.");
        }

        var rawResultsType = parameters[0].ParameterType;
        if (!rawResultsType.IsGenericType || rawResultsType.GetGenericTypeDefinition() != typeof(IReadOnlyCollection<>))
        {
            throw new InvalidOperationException($"Query type '{queryType.FullName}' must declare MapResult with an IReadOnlyCollection<T> parameter.");
        }

        return rawResultsType.GetGenericArguments()[0];
    }

    /// <summary>
    /// Executes the raw SQL with a statically known row type and maps the raw rows to the public result type.
    /// </summary>
    private static async Task<TResult> InvokeTypedAsync<TResult, TRow>(
        IQueryExecutor queryExecutor,
        IQuery<TResult> query,
        object? parameters,
        MethodInfo mapResultMethod,
        QueryCardinality cardinality,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<TRow> rawResults = cardinality switch
        {
            QueryCardinality.ZeroOrMore => await queryExecutor.QueryAsync<TRow>(query.RawSql, parameters, null, cancellationToken),
            QueryCardinality.One => new[] { await queryExecutor.QuerySingleAsync<TRow>(query.RawSql, parameters, null, cancellationToken) },
            QueryCardinality.ZeroOrOne => await QuerySingleOrDefaultAsCollectionAsync<TRow>(queryExecutor, query.RawSql, parameters, cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(cardinality))
        };

        return (TResult) mapResultMethod.Invoke(query, new object?[] { rawResults })!;
    }

    /// <summary>
    /// Executes a single-or-default query and normalizes the optional row into a collection for <c>MapResult</c>.
    /// </summary>
    private static async Task<IReadOnlyCollection<TRow>> QuerySingleOrDefaultAsCollectionAsync<TRow>(
        IQueryExecutor queryExecutor,
        string rawSql,
        object? parameters,
        CancellationToken cancellationToken)
    {
        var rawResult = await queryExecutor.QuerySingleOrDefaultAsync<TRow>(rawSql, parameters, null, cancellationToken);
        return rawResult is null ? Array.Empty<TRow>() : new[] { rawResult };
    }
}
