using System;
using System.Reflection;
using System.Threading.Tasks;
using Dapper;
using IOKode.OpinionatedFramework.Resources.Attributes;
using IOKode.OpinionatedFramework.Tests.NHibernate.Postgres.Config;
using Xunit;
using Xunit.Abstractions;

namespace IOKode.OpinionatedFramework.Tests.NHibernate.Postgres;

[Collection(nameof(NHibernateTestsFixtureCollection))]
public class QueryObjectTests(NHibernateTestsFixture fixture, ITestOutputHelper outputHelper) : NHibernateTestsBase(fixture, outputHelper)
{
    [Fact]
    public async Task Single()
    {
        // Arrange
        await npgsqlClient.ExecuteAsync("CREATE TABLE IF NOT EXISTS users (id INTEGER PRIMARY KEY, name TEXT NOT NULL);");
        await npgsqlClient.ExecuteAsync("INSERT INTO users (id, name) VALUES (1, 'Ivan');");

        // Act and Assert
        var result = await GetUserNameQuery.InvokeAsync(1, default);
        var attribute = typeof(GetUserNameQuery).GetCustomAttribute<RetrieveResourceAttribute>();
        Assert.Equal("Ivan", result.Name);
        Assert.NotNull(attribute);
        Assert.Equal("active user", attribute.Resource);
        Assert.Equal("name", attribute.Key);
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await GetUserNameQuery.InvokeAsync(2, default));

        // Post Assert
        await npgsqlClient.ExecuteAsync("DROP TABLE users;");
    }

    [Fact]
    public async Task SingleOrDefault()
    {
        // Arrange
        await npgsqlClient.ExecuteAsync("CREATE TABLE IF NOT EXISTS users (id INTEGER PRIMARY KEY, name TEXT NOT NULL);");
        await npgsqlClient.ExecuteAsync("INSERT INTO users (id, name) VALUES (1, 'Ivan');");

        // Act
        var result1 = await GetUserIfExistsQuery.InvokeAsync("Ivan", default);
        var result2 = await GetUserIfExistsQuery.InvokeAsync("Marta", default);

        // Assert
        Assert.NotNull(result1);
        Assert.Equal(1, result1.Id);
        Assert.Equal("Ivan", result1.Name);
        Assert.Null(result2);

        // Post Assert
        await npgsqlClient.ExecuteAsync("DROP TABLE users;");
    }

    [Fact]
    public async Task Many()
    {
        // Arrange
        await npgsqlClient.ExecuteAsync("CREATE TYPE address_type AS (line VARCHAR(100), region VARCHAR(100), country_code VARCHAR(3));");
        await npgsqlClient.ReloadTypesAsync();
        await npgsqlClient.ExecuteAsync("CREATE TABLE IF NOT EXISTS users (id INTEGER PRIMARY KEY, name TEXT NOT NULL, address address_type NOT NULL);");
        await npgsqlClient.ExecuteAsync("INSERT INTO users (id, name, address) VALUES (1, 'Ivan', ('Fake St. 123', 'Springfield', 'USA'));");
        await npgsqlClient.ExecuteAsync("INSERT INTO users (id, name, address) VALUES (2, 'Ivan', ('Fake St. 123', 'Springfield', 'EST'));");
        await npgsqlClient.ExecuteAsync("INSERT INTO users (id, name, address) VALUES (3, 'Marta', ('Fake St. 123', 'Springfield', 'USA'));");
        await npgsqlClient.ExecuteAsync("INSERT INTO users (id, name, address) VALUES (4, 'Javier', ('Fake St. 123', 'Springfield', 'ESP'));");

        // Act
        var collectionWithoutFilter = await GetUserByFilterQuery.InvokeAsync(null, null, default);
        var collectionByAddress = await GetUserByFilterQuery.InvokeAsync(null, new Address("Fake St. 123", "Springfield", new CountryCode("ESP")), default);
        var collectionByName = await GetUserByFilterQuery.InvokeAsync("Ivan", null, default);
        var collectionNameAndAddress = await GetUserByFilterQuery.InvokeAsync("Ivan", new Address("Fake St. 123", "Springfield", new CountryCode("USA")), default);

        // Assert
        Assert.Equal(4, collectionWithoutFilter.Count);

        var singleResultByAddress = Assert.Single(collectionByAddress);
        Assert.Equal("Javier", singleResultByAddress.UserName);

        Assert.Equal(2, collectionByName.Count);
        Assert.Collection(collectionByName,
            result => Assert.Equal("USA", result.Address.CountryCode.IsoCode),
            result => Assert.Equal("EST", result.Address.CountryCode.IsoCode));

        var singleResultByNameAndAddress = Assert.Single(collectionNameAndAddress);
        Assert.Equal("Ivan", singleResultByNameAndAddress.UserName);
        Assert.Equal("USA", singleResultByNameAndAddress.Address.CountryCode.IsoCode);

        // Post Assert
        await npgsqlClient.ExecuteAsync("DROP TABLE users;");
        await npgsqlClient.ExecuteAsync("DROP TYPE address_type;");
    }
}