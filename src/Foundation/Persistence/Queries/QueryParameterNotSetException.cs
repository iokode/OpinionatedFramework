using System;

namespace IOKode.OpinionatedFramework.Persistence.Queries;

/// <summary>
/// Thrown when a query reaches its executor and a parameter marked with <see cref="SetByMiddlewareAttribute"/>
/// still has no value, because no middleware set it.
/// </summary>
public class QueryParameterNotSetException(string parameterName)
    : Exception($"The query parameter '{parameterName}' is set by a middleware, but it had no value when the query reached the executor.")
{
    /// <summary>
    /// The name of the parameter that has no value.
    /// </summary>
    public string ParameterName { get; } = parameterName;
}
