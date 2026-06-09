using System.Collections.Generic;
using System.Linq;

namespace IOKode.OpinionatedFramework.Tests.NHibernate.Postgres.Config.Queries;

public record MappedUsersPageFilter
{
    public string? Name { get; init; }
    public int Skip { get; init; }
    public int Take { get; init; }
}

public record MappedUsersPage
{
    public required int Total { get; init; }
    public required IReadOnlyCollection<MappedUser> Users { get; init; }
}

public record MappedUser
{
    public required int Id { get; init; }
    public required string Name { get; init; }
}

public partial class ListMappedPaginatedUsersWithTotalQuery
{
    private partial ListMappedPaginatedUsersWithTotalQueryParameters MapParameters()
    {
        return new ListMappedPaginatedUsersWithTotalQueryParameters
        {
            Name = Filter.Name,
            Skip = Filter.Skip,
            Take = Filter.Take
        };
    }

    private partial MappedUsersPage MapResult(int filteredTotal, IReadOnlyCollection<FilteredUserResult> filteredUsers)
    {
        return new MappedUsersPage
        {
            Total = filteredTotal,
            Users = filteredUsers
                .Select(user => new MappedUser
                {
                    Id = user.Id,
                    Name = user.Name
                })
                .ToList()
        };
    }
}
