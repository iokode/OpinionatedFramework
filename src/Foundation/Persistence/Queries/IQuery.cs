using System;
using System.Collections.Generic;

namespace IOKode.OpinionatedFramework.Persistence.Queries;

/// <summary>
/// Represents a query object that describes a SQL query and its public result type.
/// </summary>
/// <typeparam name="TResult">The public result type returned when the query is invoked.</typeparam>
public interface IQuery<TResult>
{
    /// <summary>
    /// Gets the raw SQL query to execute.
    /// </summary>
    string RawSql { get; }

    /// <summary>
    /// Gets the directives associated with the query.
    /// </summary>
    IReadOnlyList<string> Directives { get; }

    /// <summary>
    /// Gets the expected result cardinality for the query.
    /// </summary>
    QueryCardinality Cardinality { get; }

    /// <summary>
    /// Gets the public result type returned when the query is invoked.
    /// </summary>
    Type ResultType => typeof(TResult);
}
