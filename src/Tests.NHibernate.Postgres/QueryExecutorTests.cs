using System.Threading.Tasks;
using Dapper;
using IOKode.OpinionatedFramework.ContractImplementations.NHibernate;
using IOKode.OpinionatedFramework.ContractImplementations.NHibernate.Postgres;
using IOKode.OpinionatedFramework.Ensuring;
using IOKode.OpinionatedFramework.Ensuring.Ensurers;
using IOKode.OpinionatedFramework.Persistence.Queries;
using IOKode.OpinionatedFramework.Tests.NHibernate.Postgres.Config;
using Xunit;
using Xunit.Abstractions;

namespace IOKode.OpinionatedFramework.Tests.NHibernate.Postgres;

[Collection(nameof(NHibernateTestsFixtureCollection))]
public class QueryExecutorTests(NHibernateTestsFixture fixture, ITestOutputHelper outputHelper) : NHibernateTestsBase(fixture, outputHelper)
{
    [Fact]
    public async Task QueryWithComplexValueObject()
    {
        // Arrange
        UserTypeMapper.AddUserType<CountryCode, CountryCodeType>();
        var queryExecutor = Locator.Resolve<IQueryExecutor>();
        await npgsqlClient.ExecuteAsync("CREATE TABLE IF NOT EXISTS users (id INTEGER PRIMARY KEY, country_code VARCHAR(3) NOT NULL);");
        await npgsqlClient.ExecuteAsync("INSERT INTO users (id, country_code) VALUES (1, 'EST');");

        // Act
        var result = await queryExecutor.QuerySingleAsync<UserResult>("select * from users where id = 1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("EST", result.CountryCode.IsoCode);

        // Post Assert
        await npgsqlClient.ExecuteAsync("DROP TABLE users;");
    }
    
    [Fact]
    public async Task QueryWithCompositeValueObject()
    {
        // Arrange
        var queryExecutor = Locator.Resolve<IQueryExecutor>();
        await npgsqlClient.ExecuteAsync("CREATE TYPE address_type AS (line VARCHAR(100), region VARCHAR(100), country_code VARCHAR(3));");
        await npgsqlClient.ReloadTypesAsync();
        await npgsqlClient.ExecuteAsync("CREATE TABLE IF NOT EXISTS users (id INTEGER PRIMARY KEY, address address_type NOT NULL);");
        await npgsqlClient.ExecuteAsync("INSERT INTO users (id, address) VALUES (1, ('Fake St. 123', 'Springfield', 'USA'));");

        // Act
        var result = await queryExecutor.QuerySingleAsync<UserResult2>("select * from users where id = 1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("USA", result.Address.CountryCode.IsoCode);

        // Post Assert
        await npgsqlClient.ExecuteAsync("DROP TABLE users;");
    }
}

public record CountryCode
{
    public string IsoCode { get; private set; }

    private CountryCode()
    {
    }

    public CountryCode(string code)
    {
        Ensure.String.Alpha(code, AlphaOptions.Default)
            .ElseThrowsIllegalArgument("The provided code must only contain alphabetic characters.", nameof(code));
        Ensure.String.Length(code, 3)
            .ElseThrowsIllegalArgument("The provided code must be exactly 3 characters long.", nameof(code));

        IsoCode = code;
    }
}

public record Address
{
    public string Line { get; private set; }
    public string Region { get; private set; }
    public CountryCode CountryCode { get; private set; }

    public Address(string line, string region, CountryCode countryCode)
    {
        Line = line;
        Region = region;
        CountryCode = countryCode;
    }
}

public record UserResult
{
    public required int Id { get; init; }
    public required CountryCode CountryCode { get; init; }
}

public record UserResult2
{
    public required int Id { get; init; }
    public required Address Address { get; init; }
}