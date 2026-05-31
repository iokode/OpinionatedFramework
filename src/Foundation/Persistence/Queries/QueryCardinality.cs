namespace IOKode.OpinionatedFramework.Persistence.Queries;

/// <summary>
/// Defines the expected number of rows returned by a query.
/// </summary>
public enum QueryCardinality
{
    /// <summary>
    /// The query can return zero or more rows.
    /// </summary>
    ZeroOrMore,

    /// <summary>
    /// The query must return exactly one row.
    /// </summary>
    One,

    /// <summary>
    /// The query can return zero or one row.
    /// </summary>
    ZeroOrOne
}
