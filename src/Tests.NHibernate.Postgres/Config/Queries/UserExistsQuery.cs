using System.Collections.Generic;
using System.Linq;

namespace IOKode.OpinionatedFramework.Tests.NHibernate.Postgres.Config.Queries;

public partial class UserExistsQuery
{
    private partial bool MapResult(IReadOnlyCollection<UserExistsQueryResult> rawResults)
    {
        var result = rawResults.FirstOrDefault();
        return result != null;
    }
}
