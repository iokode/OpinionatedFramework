using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace IOKode.OpinionatedFramework.Tests.NHibernate.Postgres.Config.Queries;

internal partial class ListUsersByDescendingIdQuery
{
    private partial IReadOnlyCollection<ListUsersByDescendingIdQueryResult> MapResult(
        IReadOnlyCollection<ListUsersByDescendingIdQueryResult> rawResults)
    {
        return rawResults.OrderByDescending(result => result.Id).ToImmutableList();
    }
}
