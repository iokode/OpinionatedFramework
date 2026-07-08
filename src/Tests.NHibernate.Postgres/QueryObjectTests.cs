using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using IOKode.OpinionatedFramework.Persistence.Queries;
using IOKode.OpinionatedFramework.Persistence.UnitOfWork.QueryBuilder.Exceptions;
using IOKode.OpinionatedFramework.Resources.Attributes;
using IOKode.OpinionatedFramework.ServiceLocation;
using IOKode.OpinionatedFramework.Tests.NHibernate.Postgres.Config;
using IOKode.OpinionatedFramework.Tests.NHibernate.Postgres.Config.Queries;
using Xunit;
using Xunit.Abstractions;

namespace IOKode.OpinionatedFramework.Tests.NHibernate.Postgres;

[Collection(nameof(NHibernateTestsFixtureCollection))]
public class QueryObjectTests(NHibernateTestsFixture fixture, ITestOutputHelper outputHelper) : NHibernateTestsBase(fixture, outputHelper)
{
    [Fact]
    public async Task One()
    {
        // Arrange
        await npgsqlClient.ExecuteAsync("INSERT INTO users (id, name) VALUES ('1', 'Ivan');");

        // Act and Assert
        var query = new GetUserNameQuery { Id = "1" };
        var result = await query.InvokeAsync(default);
        var attribute = typeof(GetUserNameQuery).GetCustomAttribute<RetrieveResourceAttribute>();
        Assert.Equal("Ivan", result.Name);
        Assert.Contains("generate", query.Directives);
        Assert.Contains("parameter string id", query.Directives);
        Assert.Contains("cardinality one", query.Directives);
        Assert.Single(query.ResultSets);
        Assert.Null(query.ResultSets[0].Name);
        Assert.Contains("cardinality one", query.ResultSets[0].Directives);
        Assert.Contains("result string name", query.ResultSets[0].Directives);
        Assert.NotNull(attribute);
        Assert.Equal("active user", attribute.Resource);
        Assert.Equal("name", attribute.Key);
        await Assert.ThrowsAsync<EmptyResultException>(async () => await new GetUserNameQuery { Id = "2" }.InvokeAsync(default));
    }

    [Fact]
    public async Task ZeroOrOne()
    {
        // Arrange
        await npgsqlClient.ExecuteAsync("INSERT INTO users (id, name) VALUES ('1', 'Ivan');");

        // Act
        var queryExecutor = Locator.Resolve<IQueryExecutor>();
        var result1 = await queryExecutor.InvokeAsync(new GetUserIfExistsQuery { Name = "Ivan" }, default);
        var result2 = await new GetUserIfExistsQuery { Name = "Marta" }.InvokeAsync(default);

        // Assert
        Assert.NotNull(result1);
        Assert.Equal(1, result1.Id);
        Assert.Equal("Ivan", result1.Name);
        Assert.Null(result2);
    }

    [Fact]
    public async Task ZeroOrMore()
    {
        // Arrange
        await npgsqlClient.ExecuteAsync("INSERT INTO users (id, name, address) VALUES (1, 'Ivan', ('Fake St. 123', 'Springfield', 'USA'));");
        await npgsqlClient.ExecuteAsync("INSERT INTO users (id, name, address) VALUES (2, 'Ivan', ('Fake St. 123', 'Springfield', 'EST'));");
        await npgsqlClient.ExecuteAsync("INSERT INTO users (id, name, address) VALUES (3, 'Marta', ('Fake St. 123', 'Springfield', 'USA'));");
        await npgsqlClient.ExecuteAsync("INSERT INTO users (id, name, address) VALUES (4, 'Javier', ('Fake St. 123', 'Springfield', 'ESP'));");

        // Act
        var collectionWithoutFilter = await new GetUserByFilterQuery().InvokeAsync(default);
        var collectionByAddress = await new GetUserByFilterQuery
        {
            Address = new Address("Fake St. 123", "Springfield", new CountryCode("ESP"))
        }.InvokeAsync(default);
        var collectionByName = await new GetUserByFilterQuery { UserName = "Ivan" }.InvokeAsync(default);
        var collectionNameAndAddress = await new GetUserByFilterQuery
        {
            UserName = "Ivan",
            Address = new Address("Fake St. 123", "Springfield", new CountryCode("USA"))
        }.InvokeAsync(default);

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
    }

    [Fact]
    public async Task Map_WithResultCollection()
    {
        // Arrange
        await npgsqlClient.ExecuteAsync("INSERT INTO users (id, name) VALUES ('1', 'Ivan');");
        await npgsqlClient.ExecuteAsync("INSERT INTO users (id, name) VALUES ('2', 'Ivan');");
        await npgsqlClient.ExecuteAsync("INSERT INTO users (id, name) VALUES ('3', 'Marta');");

        // Act
        var results = await new ListUsersByDescendingIdQuery().InvokeAsync(CancellationToken.None);

        // Assert
        Assert.Equal(3, results.Count);
        Assert.Equal(new[] { 3, 2, 1 }, results.Select(user => user.Id));
    }

    [Fact]
    public async Task Map_WithParameters()
    {
        // Arrange
        await npgsqlClient.ExecuteAsync("INSERT INTO users (id, name, address) VALUES ('1', 'Ivan', ('Fake St. 123', 'Springfield', 'USA'));");
        await npgsqlClient.ExecuteAsync("INSERT INTO users (id, name, address) VALUES ('2', 'Ivan', ('Fake St. 123', 'Springfield', 'EST'));");
        await npgsqlClient.ExecuteAsync("INSERT INTO users (id, name, address) VALUES ('3', 'Marta', ('Fake St. 123', 'Springfield', 'USA'));");
        await npgsqlClient.ExecuteAsync("INSERT INTO users (id, name, address) VALUES ('4', 'Javier', ('Fake St. 123', 'Springfield', 'ESP'));");
        var filters1 = new UsersFilters
        {
            Address = new Address("Fake St. 123", "Springfield", new CountryCode("USA")),
            Name = "Ivan"
        };
        var filters2 = new UsersFilters
        {
            Name = "Ivan"
        };

        // Act
        var usersWithNoFilters = await new GetUsersWithFiltersQuery().InvokeAsync(CancellationToken.None);
        var usersWithFilters1 = await new GetUsersWithFiltersQuery { Filters = filters1 }.InvokeAsync(CancellationToken.None);
        var usersWithFilters2 = await new GetUsersWithFiltersQuery { Filters = filters2 }.InvokeAsync(CancellationToken.None);

        // Assert
        Assert.Equal(new[] { 1, 2, 3, 4 }, usersWithNoFilters.Select(user => user.Id));
        Assert.Equal(new[] { 1 }, usersWithFilters1.Select(user => user.Id));
        Assert.Equal(new[] { 1, 2 }, usersWithFilters2.Select(user => user.Id));
    }

    [Fact]
    public async Task Map_WithResult()
    {
        // Arrange
        await npgsqlClient.ExecuteAsync("INSERT INTO users (id, name) VALUES ('1', 'Ivan');");

        // Act
        var user1Exists = await new UserExistsQuery { Name = "Ivan" }.InvokeAsync(CancellationToken.None);
        var user2Exists = await new UserExistsQuery { Name = "Marta" }.InvokeAsync(CancellationToken.None);

        // Assert
        Assert.True(user1Exists);
        Assert.False(user2Exists);
    }

    [Fact]
    public async Task Map_WithParametersAndResultAndCount()
    {
        // Arrange
        await npgsqlClient.ExecuteAsync("INSERT INTO users (id, name, address) VALUES ('1', 'Ivan', ('Fake St. 123', 'Springfield', 'USA'));");
        await npgsqlClient.ExecuteAsync("INSERT INTO users (id, name, address) VALUES ('2', 'Ivan', ('Fake St. 123', 'Springfield', 'EST'));");
        await npgsqlClient.ExecuteAsync("INSERT INTO users (id, name, address) VALUES ('3', 'Marta', ('Fake St. 123', 'Springfield', 'USA'));");
        await npgsqlClient.ExecuteAsync("INSERT INTO users (id, name, address) VALUES ('4', 'Javier', ('Fake St. 123', 'Springfield', 'ESP'));");
        var filters1 = new UsersFilters
        {
            Address = new Address("Fake St. 123", "Springfield", new CountryCode("USA")),
            Name = "Ivan"
        };
        var filters2 = new UsersFilters
        {
            Name = "Ivan"
        };

        // Act
        var countWithNoFilters = await new UsersCountWithFiltersQuery().InvokeAsync(CancellationToken.None);
        var countWithFilters1 = await new UsersCountWithFiltersQuery { Filters = filters1 }.InvokeAsync(CancellationToken.None);
        var countWithFilters2 = await new UsersCountWithFiltersQuery { Filters = filters2 }.InvokeAsync(CancellationToken.None);

        // Assert
        Assert.Equal(4, countWithNoFilters);
        Assert.Equal(1, countWithFilters1);
        Assert.Equal(2, countWithFilters2);
    }

    [Fact]
    public async Task ScalarResult_ReturnsSingleValue()
    {
        // Arrange
        await npgsqlClient.ExecuteAsync("INSERT INTO users (id, name) VALUES ('1', 'Ivan');");
        await npgsqlClient.ExecuteAsync("INSERT INTO users (id, name) VALUES ('2', 'Marta');");

        // Act
        var count = await new UsersScalarCountQuery().InvokeAsync(CancellationToken.None);

        // Assert
        Assert.Equal(2, count);
    }

    [Fact]
    public async Task MultipleResultSets_WithScalarAndCollection()
    {
        // Arrange
        await npgsqlClient.ExecuteAsync("INSERT INTO users (id, name) VALUES ('1', 'Ivan');");
        await npgsqlClient.ExecuteAsync("INSERT INTO users (id, name) VALUES ('2', 'Ivan');");
        await npgsqlClient.ExecuteAsync("INSERT INTO users (id, name) VALUES ('3', 'Marta');");
        await npgsqlClient.ExecuteAsync("INSERT INTO users (id, name) VALUES ('4', 'Marta');");
        await npgsqlClient.ExecuteAsync("INSERT INTO users (id, name) VALUES ('5', 'Javier');");
        await npgsqlClient.ExecuteAsync("INSERT INTO users (id, name) VALUES ('6', 'Javier');");

        // Act
        var page1 = await new ListPaginatedUsersWithTotalQuery { Skip = 0, Take = 2 }.InvokeAsync(CancellationToken.None);
        var page2 = await new ListPaginatedUsersWithTotalQuery { Skip = 2, Take = 1 }.InvokeAsync(CancellationToken.None);
        var page3 = await new ListPaginatedUsersWithTotalQuery { Skip = 3, Take = 3 }.InvokeAsync(CancellationToken.None);
        var page4 = await new ListPaginatedUsersWithTotalQuery { Skip = 6, Take = 3 }.InvokeAsync(CancellationToken.None);

        // Assert
        var query = new ListPaginatedUsersWithTotalQuery { Skip = 0, Take = 1 };
        Assert.Contains("custom_global_directive", query.Directives);
        Assert.DoesNotContain("custom_total_directive", query.Directives);
        Assert.DoesNotContain("custom_users_directive", query.Directives);
        Assert.Equal(new[] { "total", "users" }, query.ResultSets.Select(resultSet => resultSet.Name));
        Assert.Contains("custom_total_directive", query.ResultSets[0].Directives);
        Assert.Contains("scalar_result int total", query.ResultSets[0].Directives);
        Assert.Contains("custom_users_directive", query.ResultSets[1].Directives);
        Assert.Contains("result string name", query.ResultSets[1].Directives);

        Assert.Equal(6, page1.Total);
        Assert.Equal(2, page1.Users.Count);
        Assert.True(page1.Users.All(result => result.Name == "Ivan"));
        
        Assert.Equal(6, page2.Total);
        Assert.Single(page2.Users);
        Assert.Equal("Marta", page2.Users.Single().Name);
        
        Assert.Equal(6, page3.Total);
        Assert.Equal(3, page3.Users.Count);
        Assert.Equal(new[] { "Marta", "Javier", "Javier" }, page3.Users.Select(result => result.Name));

        Assert.Equal(6, page4.Total);
        Assert.Empty(page4.Users);
    }

    [Fact]
    public async Task Map_WithParametersAndMultipleResultSets()
    {
        // Arrange
        await npgsqlClient.ExecuteAsync("INSERT INTO users (id, name) VALUES ('1', 'Ivan');");
        await npgsqlClient.ExecuteAsync("INSERT INTO users (id, name) VALUES ('2', 'Ivan');");
        await npgsqlClient.ExecuteAsync("INSERT INTO users (id, name) VALUES ('3', 'Marta');");

        // Act
        var result = await new ListMappedPaginatedUsersWithTotalQuery
        {
            Filter = new MappedUsersPageFilter
            {
                Name = "Ivan",
                Skip = 1,
                Take = 1
            }
        }.InvokeAsync(CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Total);
        var user = Assert.Single(result.Users);
        Assert.Equal(2, user.Id);
        Assert.Equal("Ivan", user.Name);
    }
}
