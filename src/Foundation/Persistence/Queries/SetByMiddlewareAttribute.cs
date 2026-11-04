using System;

namespace IOKode.OpinionatedFramework.Persistence.Queries;

/// <summary>
/// Marks a query parameter whose value is not given by whoever builds the query, but set by a
/// <see cref="QueryMiddleware"/> through <see cref="IQueryExecutionContext.SetParameter"/> before the query is executed.
/// </summary>
/// <remarks>
/// The SQL query objects generator applies it to the properties declared with
/// <c>-- @parameter &lt;type&gt; &lt;name&gt; from middleware</c>, both on the query object and on its parameters object.
/// The resource controller generator leaves those properties out of the generated actions, and the query executor
/// throws a <see cref="QueryParameterNotSetException"/> when one of them still has no value when the query reaches it.
/// </remarks>
[AttributeUsage(AttributeTargets.Property)]
public sealed class SetByMiddlewareAttribute : Attribute;
