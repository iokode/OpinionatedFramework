using System;
using System.Collections.Generic;

namespace IOKode.OpinionatedFramework.Persistence.Queries;

/// <summary>
/// Describes a SQL result set that must be executed and mapped by a query executor.
/// </summary>
public sealed class QueryResultSet
{
    /// <summary>
    /// The result set name. Null for implicit single-result-set queries.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// The SQL text for this result set.
    /// </summary>
    public required string RawSql { get; init; }

    /// <summary>
    /// The directives associated with this result set.
    /// </summary>
    public required IReadOnlyList<string> Directives { get; init; }

    /// <summary>
    /// The expected number of rows returned by this result set.
    /// </summary>
    public required QueryCardinality Cardinality { get; init; }

    /// <summary>
    /// The shape of each row returned by this result set.
    /// </summary>
    public required QueryResultShape Shape { get; init; }

    /// <summary>
    /// The CLR type of each row or scalar value returned by this result set.
    /// </summary>
    public required Type ResultType { get; init; }

    /// <summary>
    /// The SQL column name for scalar result sets.
    /// </summary>
    public string? ScalarColumnName { get; init; }
}
