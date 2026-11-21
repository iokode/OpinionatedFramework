namespace IOKode.OpinionatedFramework.Tests.NHibernate.Postgres.Config.Queries;

public partial class RefusedResultQuery
{
    private partial string MapResult(RefusedResultQueryResult result)
    {
        throw new MappingRefusedException($"The name '{result.Name}' is refused.");
    }
}
