using System;
using System.Collections.Generic;

namespace IOKode.OpinionatedFramework.Persistence.Queries;

/// <summary>
/// Represents the execution context for one SQL result set within a query execution.
/// </summary>
public interface IQueryResultSetExecutionContext
{
    /// <summary>
    /// The result set name. Null for implicit single-result-set queries.
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// The directives associated with this result set.
    /// </summary>
    public IReadOnlyList<string> Directives { get; }

    /// <summary>
    /// The expected number of rows returned by this result set.
    /// </summary>
    public QueryCardinality Cardinality { get; }

    /// <summary>
    /// The shape of each row returned by this result set.
    /// </summary>
    public QueryResultShape Shape { get; }

    /// <summary>
    /// The CLR type of each row or scalar value returned by this result set.
    /// </summary>
    public Type ResultType { get; }

    /// <summary>
    /// The SQL column name for scalar result sets.
    /// </summary>
    public string? ScalarColumnName { get; }

    /// <summary>
    /// The raw SQL query for this result set.
    /// </summary>
    /// <remarks>It can be modified from a query middleware.</remarks>
    public string RawQuery { get; set; }

    /// <summary>
    /// Retrieves the collection of results obtained after executing this result set.
    /// </summary>
    public IReadOnlyList<object> Results { get; }
}
