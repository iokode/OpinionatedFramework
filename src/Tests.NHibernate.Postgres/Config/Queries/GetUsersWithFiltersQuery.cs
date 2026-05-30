using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IOKode.OpinionatedFramework.Tests.NHibernate.Postgres.Config.Queries;

public static partial class GetUsersWithFiltersQuery
{
    public static Task<IReadOnlyCollection<GetUsersWithFiltersQueryResult>> InvokeAsync(CancellationToken cancellationToken)
    {
        return InvokeAsync(cancellationToken, null);
    }

    public static partial async Task<IReadOnlyCollection<GetUsersWithFiltersQueryResult>> InvokeAsync(CancellationToken cancellationToken, UsersFilters? filters)
    {
        return await QueryAsync(filters?.Name, filters?.Address, cancellationToken);
    }
}
