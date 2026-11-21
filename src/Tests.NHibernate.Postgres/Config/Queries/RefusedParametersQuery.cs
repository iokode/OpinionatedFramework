namespace IOKode.OpinionatedFramework.Tests.NHibernate.Postgres.Config.Queries;

public partial class RefusedParametersQuery
{
    private partial RefusedParametersQueryParameters MapParameters()
    {
        throw new MappingRefusedException($"The parameter '{this.Refused}' is refused.");
    }

    private partial int MapResult(RefusedParametersQueryResult result)
    {
        return result.Count;
    }
}
