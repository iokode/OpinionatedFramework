namespace IOKode.OpinionatedFramework.Persistence.Queries;

/// <summary>
/// Defines the shape of each row returned by a query result set.
/// </summary>
public enum QueryResultShape
{
    /// <summary>
    /// Each row is mapped to an object with writable properties.
    /// </summary>
    Object,

    /// <summary>
    /// Each row is mapped to a single scalar value.
    /// </summary>
    Scalar
}
