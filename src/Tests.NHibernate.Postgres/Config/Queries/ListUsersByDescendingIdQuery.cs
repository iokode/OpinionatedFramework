using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace IOKode.OpinionatedFramework.Tests.NHibernate.Postgres.Config.Queries;

internal partial class ListUsersByDescendingIdQuery
{
    private partial IReadOnlyCollection<ListUsersByDescendingIdQueryResult> MapResult(
        IReadOnlyCollection<ListUsersByDescendingIdQueryResult> result)
    {
        return result.OrderByDescending(resultItem => resultItem.Id).ToImmutableList();
    }
}
