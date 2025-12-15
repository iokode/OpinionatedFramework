namespace IOKode.OpinionatedFramework.Tests.NHibernate.Postgres.Config;

public record UsersFilters
{
    public string? Name { get; init; }
    public Address? Address { get; init; }
}