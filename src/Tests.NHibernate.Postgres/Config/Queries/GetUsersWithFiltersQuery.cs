namespace IOKode.OpinionatedFramework.Tests.NHibernate.Postgres.Config.Queries;

public partial class GetUsersWithFiltersQuery
{
    private partial GetUsersWithFiltersQueryParameters MapParameters()
    {
        return new GetUsersWithFiltersQueryParameters
        {
            Name = this.Filters?.Name,
            Address = this.Filters?.Address
        };
    }
}
