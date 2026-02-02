using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IOKode.OpinionatedFramework.Tests.NHibernate.Postgres.Config.Queries;

internal partial class ListUsersByDescendingIdQuery
{
    public static partial async Task<IReadOnlyCollection<ListUsersByDescendingIdQueryResult>> InvokeAsync(CancellationToken cancellationToken)
    {
        var results = await QueryAsync(cancellationToken);
        return results.OrderByDescending(result => result.Id).ToImmutableList();
    }
}