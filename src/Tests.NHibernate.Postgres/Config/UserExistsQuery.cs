using System.Threading;
using System.Threading.Tasks;

namespace IOKode.OpinionatedFramework.Tests.NHibernate.Postgres.Config;

public partial class UserExistsQuery
{
    public static partial async Task<bool> InvokeAsync(string name, CancellationToken cancellationToken)
    {
        var result = await QueryAsync(name, cancellationToken);
        return result != null;
    }
}