using System.Threading;
using System.Threading.Tasks;

namespace IOKode.OpinionatedFramework.Tests.NHibernate.Postgres.Config;

public partial class UsersCountWithFiltersQuery
{
    public static partial async Task<int> InvokeAsync(UsersFilters? filters, CancellationToken cancellationToken)
    {
        var result = await QueryAsync(filters?.Name, filters?.Address, cancellationToken);
        return result.Count;
    }

    public static async Task<int> InvokeAsync(CancellationToken cancellationToken)
    {
        var result = await InvokeAsync(null, cancellationToken);
        return result;
    }
}