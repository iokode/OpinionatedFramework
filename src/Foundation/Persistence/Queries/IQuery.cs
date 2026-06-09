using System;
using System.Collections.Generic;

namespace IOKode.OpinionatedFramework.Persistence.Queries;

/// <summary>
/// Represents a query object that describes the original SQL text, its global directives, its result sets,
/// and its public result type.
/// </summary>
/// <typeparam name="TResult">The public result type returned when the query is invoked.</typeparam>
public interface IQuery<TResult>
{
    /// <summary>
    /// Gets the original raw SQL text for the query, including every result set block.
    /// </summary>
    string RawSql { get; }

    /// <summary>
    /// Gets the global directives associated with the query.
    /// </summary>
    IReadOnlyList<string> Directives { get; }

    /// <summary>
    /// Gets the SQL result sets that compose this query. Each result set contains its own SQL text,
    /// directives, cardinality and result shape.
    /// </summary>
    IReadOnlyList<QueryResultSet> ResultSets { get; }

    /// <summary>
    /// Gets the public result type returned when the query is invoked.
    /// </summary>
    Type ResultType => typeof(TResult);
}
