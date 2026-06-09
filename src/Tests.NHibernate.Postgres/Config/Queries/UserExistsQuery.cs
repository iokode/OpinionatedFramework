namespace IOKode.OpinionatedFramework.Tests.NHibernate.Postgres.Config.Queries;

public partial class UserExistsQuery
{
    private partial bool MapResult(UserExistsQueryResult? result)
    {
        return result != null;
    }
}
