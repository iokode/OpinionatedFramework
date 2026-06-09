using System.Collections.Generic;

namespace IOKode.OpinionatedFramework.Persistence.Queries;

/// <summary>
/// Represents the global SQL block and result set SQL blocks of a query.
/// </summary>
public class SqlQueryBlocks
{
    /// <summary>
    /// Gets the SQL text that appears before the first explicit result set block.
    /// </summary>
    public required string GlobalRawSql { get; init; }

    /// <summary>
    /// Gets the directives that appear before the first explicit result set block.
    /// </summary>
    public required IReadOnlyList<string> GlobalDirectives { get; init; }

    /// <summary>
    /// Gets a value indicating whether the query declares explicit result set blocks.
    /// </summary>
    public required bool HasExplicitResultSets { get; init; }

    /// <summary>
    /// Gets the result set blocks of the query.
    /// </summary>
    public required IReadOnlyList<SqlQueryResultSetBlock> ResultSets { get; init; }
}
