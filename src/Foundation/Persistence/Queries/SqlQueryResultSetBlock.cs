using System.Collections.Generic;

namespace IOKode.OpinionatedFramework.Persistence.Queries;

/// <summary>
/// Represents the SQL text and directives for a query result set block.
/// </summary>
public class SqlQueryResultSetBlock
{
    /// <summary>
    /// Gets the SQL text that belongs to this result set block.
    /// </summary>
    public required string RawSql { get; init; }

    /// <summary>
    /// Gets the directives that belong to this result set block.
    /// </summary>
    public required IReadOnlyList<string> Directives { get; init; }
}
