using System;

namespace IOKode.OpinionatedFramework.Persistence.Queries;

/// <summary>
/// Represents an error in the way a query is defined.
/// </summary>
public class QueryDefinitionException(string message) : Exception(message);
