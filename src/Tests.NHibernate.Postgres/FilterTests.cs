using System.Threading.Tasks;
using Dapper;
using IOKode.OpinionatedFramework.ContractImplementations.NHibernate;
using IOKode.OpinionatedFramework.ContractImplementations.NHibernate.UnitOfWork;
using IOKode.OpinionatedFramework.Persistence.UnitOfWork;
using IOKode.OpinionatedFramework.Persistence.UnitOfWork.QueryBuilder.Filters;
using IOKode.OpinionatedFramework.Tests.NHibernate.Postgres.Config;
using Xunit;
using Xunit.Abstractions;
using NotFilter = IOKode.OpinionatedFramework.Persistence.UnitOfWork.QueryBuilder.Filters.NotFilter;

namespace IOKode.OpinionatedFramework.Tests.NHibernate.Postgres;

[Collection(nameof(NHibernateTestsFixtureCollection))]
public class FilterTests(NHibernateTestsFixture fixture, ITestOutputHelper outputHelper) : NHibernateTestsBase(fixture, outputHelper)
{
    private async Task InsertUsers()
    {
        await npgsqlClient.ExecuteAsync("INSERT INTO Users (id, name, email, is_active) VALUES ('1', 'Ivan', 'ivan@example.com', true);");
        await npgsqlClient.ExecuteAsync("INSERT INTO Users (id, name, email, is_active) VALUES ('2', 'Marta', 'marta@example.com', false);");
        await npgsqlClient.ExecuteAsync("INSERT INTO Users (id, name, email, is_active) VALUES ('3', 'Javier', 'javier@example.com', false);");
    }

    [Fact]
    public async Task EqualsFilter()
    {
        // Arrange
        var sessionFactory = configuration.BuildSessionFactory();
        await using IUnitOfWork unitOfWork = new UnitOfWork(sessionFactory);
        var entitySet = unitOfWork.GetEntitySet<User>();

        await CreateUsersTableQueryAsync();
        await InsertUsers();

        var filter = new EqualsFilter("emailAddress", "marta@example.com");

        // Act
        var users = await entitySet.ManyAsync(filter, default);

        // Assert
        Assert.Collection(users, user => Assert.Equal("Marta", user.Username));

        // Arrange post Assert
        await DropUsersTableQueryAsync();
    }

    [Fact]
    public async Task NotEqualsFilter()
    {
        // Arrange
        var sessionFactory = configuration.BuildSessionFactory();
        await using IUnitOfWork unitOfWork = new UnitOfWork(sessionFactory);
        var entitySet = unitOfWork.GetEntitySet<User>();

        await CreateUsersTableQueryAsync();
        await InsertUsers();

        var filter = new NotEqualsFilter("username", "Ivan");

        // Act
        var users = await entitySet.ManyAsync(filter, default);

        // Assert
        Assert.Collection(users,
            user => Assert.Equal("Marta", user.Username),
            user => Assert.Equal("Javier", user.Username));

        // Arrange post Assert
        await DropUsersTableQueryAsync();
    }

    [Fact]
    public async Task InFilter()
    {
        // Arrange
        var sessionFactory = configuration.BuildSessionFactory();
        await using IUnitOfWork unitOfWork = new UnitOfWork(sessionFactory);
        var entitySet = unitOfWork.GetEntitySet<User>();

        await CreateUsersTableQueryAsync();
        await InsertUsers();

        var filter = new InFilter("username", "Javier", "Marta");

        // Act
        var users = await entitySet.ManyAsync(filter, default);

        // Assert
        Assert.Collection(users,
            user => Assert.Equal("Marta", user.Username),
            user => Assert.Equal("Javier", user.Username));

        // Arrange post Assert
        await DropUsersTableQueryAsync();
    }

    [Fact]
    public async Task LikeFilter()
    {
        // Arrange
        var sessionFactory = configuration.BuildSessionFactory();
        await using IUnitOfWork unitOfWork = new UnitOfWork(sessionFactory);
        var entitySet = unitOfWork.GetEntitySet<User>();

        await CreateUsersTableQueryAsync();
        await InsertUsers();

        var filter = new LikeFilter("username", "%v%");

        // Act
        var users = await entitySet.ManyAsync(filter, default);

        // Assert
        Assert.Collection(users,
            user => Assert.Equal("Ivan", user.Username),
            user => Assert.Equal("Javier", user.Username));

        // Arrange post Assert
        await DropUsersTableQueryAsync();
    }

    [Fact]
    public async Task BetweenFilter()
    {
        // Arrange
        var sessionFactory = configuration.BuildSessionFactory();
        await using IUnitOfWork unitOfWork = new UnitOfWork(sessionFactory);
        var entitySet = unitOfWork.GetEntitySet<User>();

        await CreateUsersTableQueryAsync();
        await InsertUsers();

        var filter = new BetweenFilter("username", "Ana", "Javier");

        // Act
        var users = await entitySet.ManyAsync(filter, default);

        // Assert
        Assert.Collection(users,
            user => Assert.Equal("Ivan", user.Username),
            user => Assert.Equal("Javier", user.Username));

        // Arrange post Assert
        await DropUsersTableQueryAsync();
    }

    [Fact]
    public async Task GreaterThanFilter()
    {
        // Arrange
        var sessionFactory = configuration.BuildSessionFactory();
        await using IUnitOfWork unitOfWork = new UnitOfWork(sessionFactory);
        var entitySet = unitOfWork.GetEntitySet<User>();

        await CreateUsersTableQueryAsync();
        await InsertUsers();

        var filter = new GreaterThanFilter("username", "Ivan");

        // Act
        var users = await entitySet.ManyAsync(filter, default);

        // Assert
        Assert.Collection(users,
            user => Assert.Equal("Marta", user.Username),
            user => Assert.Equal("Javier", user.Username));

        // Arrange post Assert
        await DropUsersTableQueryAsync();
    }

    [Fact]
    public async Task LessThanFilter()
    {
        // Arrange
        var sessionFactory = configuration.BuildSessionFactory();
        await using IUnitOfWork unitOfWork = new UnitOfWork(sessionFactory);
        var entitySet = unitOfWork.GetEntitySet<User>();

        await CreateUsersTableQueryAsync();
        await InsertUsers();

        var filter = new LessThanFilter("username", "Marta");

        // Act
        var users = await entitySet.ManyAsync(filter, default);

        // Assert
        Assert.Collection(users,
            user => Assert.Equal("Ivan", user.Username),
            user => Assert.Equal("Javier", user.Username));

        // Arrange post Assert
        await DropUsersTableQueryAsync();
    }

    [Fact]
    public async Task AndFilter()
    {
        // Arrange
        var sessionFactory = configuration.BuildSessionFactory();
        await using IUnitOfWork unitOfWork = new UnitOfWork(sessionFactory);
        var entitySet = unitOfWork.GetEntitySet<User>();

        await CreateUsersTableQueryAsync();
        await InsertUsers();

        var filter = new AndFilter(
            new EqualsFilter("isActive", true),
            new LessThanFilter("username", "Marta"));

        // Act
        var users = await entitySet.ManyAsync(filter, default);

        // Assert
        Assert.Collection(users, user => Assert.Equal("Ivan", user.Username));

        // Arrange post Assert
        await DropUsersTableQueryAsync();
    }

    [Fact]
    public async Task OrFilter()
    {
        // Arrange
        var sessionFactory = configuration.BuildSessionFactory();
        await using IUnitOfWork unitOfWork = new UnitOfWork(sessionFactory);
        var entitySet = unitOfWork.GetEntitySet<User>();

        await CreateUsersTableQueryAsync();
        await InsertUsers();

        var filter = new OrFilter(
            new LikeFilter("username", "%n"),
            new EqualsFilter("emailAddress", "marta@example.com"));

        // Act
        var users = await entitySet.ManyAsync(filter, default);

        // Assert
        Assert.Collection(users,
            user => Assert.Equal("Ivan", user.Username),
            user => Assert.Equal("Marta", user.Username));

        // Arrange post Assert
        await DropUsersTableQueryAsync();
    }

    [Fact]
    public async Task NotFilter()
    {
        // Arrange
        var sessionFactory = configuration.BuildSessionFactory();
        await using IUnitOfWork unitOfWork = new UnitOfWork(sessionFactory);
        var entitySet = unitOfWork.GetEntitySet<User>();

        await CreateUsersTableQueryAsync();
        await InsertUsers();

        var filter1 = new NotFilter(
            new AndFilter(
                new EqualsFilter("isActive", true),
                new LessThanFilter("username", "Marta")
            )
        );

        var filter2 = new NotFilter(
            new OrFilter(
                new LikeFilter("username", "%n"),
                new EqualsFilter("emailAddress", "marta@example.com")
            )
        );

        // Act
        var users1 = await entitySet.ManyAsync(filter1, default);
        var users2 = await entitySet.ManyAsync(filter2, default);

        // Assert
        Assert.Collection(users1,
            user => Assert.Equal("Marta", user.Username),
            user => Assert.Equal("Javier", user.Username));
        Assert.Collection(users2, user => Assert.Equal("Javier", user.Username));

        // Arrange post Assert
        await DropUsersTableQueryAsync();
    }
}