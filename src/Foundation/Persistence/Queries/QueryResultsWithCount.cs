using System.Collections.Generic;

namespace IOKode.OpinionatedFramework.Persistence.Queries;

public class QueryResultsWithCount<TResult>
{
    public required IReadOnlyCollection<TResult> Results { get; init; }
    public required int Count { get; init; }
}