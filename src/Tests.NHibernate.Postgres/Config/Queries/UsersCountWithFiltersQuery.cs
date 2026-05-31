using System.Collections.Generic;
using System.Linq;

namespace IOKode.OpinionatedFramework.Tests.NHibernate.Postgres.Config.Queries;

public record UsersFilters
{
    public string? Name { get; init; }
    public Address? Address { get; init; }
}

public partial class UsersCountWithFiltersQuery
{
    private partial UsersCountWithFiltersQueryParameters MapParameters()
    {
        return new UsersCountWithFiltersQueryParameters
        {
            Name = this.Filters?.Name,
            Address = this.Filters?.Address
        };
    }

    private partial int MapResult(IReadOnlyCollection<UsersCountWithFiltersQueryResultWithCount> rawResults)
    {
        return rawResults.FirstOrDefault()?.Count ?? 0;
    }
}
