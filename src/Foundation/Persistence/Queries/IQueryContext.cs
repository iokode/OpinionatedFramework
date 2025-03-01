using System.Collections.Generic;
using System.Data;
using System.Threading;

namespace IOKode.OpinionatedFramework.Persistence.Queries;

/// <summary>
/// Represents the context in which a query is executed.
/// </summary>
public interface IQueryContext
{
    /// <summary>
    /// The database transaction associated with the query context, if any.
    /// </summary>
    public IDbTransaction? Transaction { get; }

    /// <summary>
    /// Indicates whether the query has already been executed.
    /// </summary>
    public bool IsQueryExecuted { get; }

    /// <summary>
    /// A collection of directives associated with the query.
    /// </summary>
    /// <remarks>A directive is a comment in the SQL file that starts with "-- @".</remarks>
    public ICollection<string> Directives { get; }

    /// <summary>
    /// Gets the collection of parameters used in the query.
    /// </summary>
    public object? Parameters { get; }

    /// <summary>
    /// The cancellation token associated with the query execution.
    /// </summary>
    /// <remarks>The same instance that the <see cref="IQueryExecutor"/> receives.</remarks>
    public CancellationToken CancellationToken { get; }

    /// <summary>
    /// The raw SQL query as a text string sourced from the associated .sql file.
    /// </summary>
    /// <remarks>It can be modified from a query middleware.</remarks>
    public string RawQuery { get; set; }

    /// <summary>
    /// Retrieves the collection of results obtained after executing the query.
    /// </summary>
    /// <exception cref="System.InvalidOperationException">Thrown when the query is not executed yed.</exception>
    public IReadOnlyCollection<object> Results { get; }
}