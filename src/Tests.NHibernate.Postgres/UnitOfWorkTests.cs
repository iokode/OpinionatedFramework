using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using IOKode.OpinionatedFramework.ContractImplementations.NHibernate.UnitOfWork;
using IOKode.OpinionatedFramework.Persistence.UnitOfWork;
using IOKode.OpinionatedFramework.Persistence.UnitOfWork.Exceptions;
using IOKode.OpinionatedFramework.Persistence.UnitOfWork.QueryBuilder.Filters;
using IOKode.OpinionatedFramework.Tests.NHibernate.Postgres.Config;
using IOKode.OpinionatedFramework.Tests.NHibernate.Postgres.Config.Entities;
using Xunit;
using Xunit.Abstractions;

namespace IOKode.OpinionatedFramework.Tests.NHibernate.Postgres;

[Collection(nameof(NHibernateTestsFixtureCollection))]
public class UnitOfWorkTests(NHibernateTestsFixture fixture, ITestOutputHelper outputHelper)
    : NHibernateTestsBase(fixture, outputHelper)
{
    [Fact]
    public async Task IdentityMap_SameInstance()
    {
        // Arrange
        await npgsqlClient.ExecuteAsync("INSERT INTO Users (id, name, email, is_active) VALUES ('1', 'Ivan', 'ivan@example.com', true);");
        await using IUnitOfWork unitOfWork = new UnitOfWork(configuration.BuildSessionFactory());
        var repository = unitOfWork.GetRepository<UserRepository>();

        // Act
        var user1 = await repository.GetByUsernameAsync("Ivan");
        var user2 = await repository.GetByEmailAddressOrDefaultAsync("ivan@example.com");

        // Assert
        Assert.Same(user1, user2);
    }

    [Fact]
    public async Task EnsureQueriedEntitiesAreTracked()
    {
        // Arrange 
        await npgsqlClient.ExecuteAsync("INSERT INTO Users (id, name, email, is_active) VALUES ('1', 'Ivan', 'ivan@example.com', true);");
        await using IUnitOfWork unitOfWork = new UnitOfWork(configuration.BuildSessionFactory());
        var repository = unitOfWork.GetRepository<UserRepository>();

        // Act and Assert
        var user = await repository.GetByUsernameAsync("Ivan");
        Assert.True(await unitOfWork.IsTrackedAsync(user));

        user.Username = "Marta";
        await unitOfWork.SaveChangesAsync(default);

        var queriedUser = await npgsqlClient.QuerySingleOrDefaultAsync<(string, string, string, bool)>("SELECT * FROM Users;");
        Assert.Equal("Marta", queriedUser.Item2);
    }

    [Fact]
    public async Task Add()
    {
        // Arrange
        await using IUnitOfWork unitOfWork = new UnitOfWork(configuration.BuildSessionFactory());
        var repository = unitOfWork.GetRepository<UserRepository>();

        // Act & Assert
        Assert.False(await unitOfWork.HasChangesAsync(default));

        var user = new User { Username = "Ivan", EmailAddress = "ivan@example.com", IsActive = true };
        await repository.AddAsync(user, default);

        Assert.True(await unitOfWork.HasChangesAsync(default));
        Assert.True(await unitOfWork.IsTrackedAsync(user, default));

        var shouldNull = await npgsqlClient.QuerySingleOrDefaultAsync<(string, string, string, bool)?>("SELECT * FROM users;");
        Assert.Null(shouldNull);

        await unitOfWork.SaveChangesAsync(default);
        var shouldSaved = await npgsqlClient.QuerySingleOrDefaultAsync<(string, string, string, bool)>("SELECT * FROM users;");
        Assert.Equal("Ivan", shouldSaved.Item2);
        await Assert.ThrowsAsync<ArgumentException>(async () => { await repository.AddAsync(user, default); });
    }

    [Fact]
    public async Task BulkDeleteByFilter()
    {
        // Arrange
        await npgsqlClient.ExecuteAsync("INSERT INTO Users (id, name, email, is_active) VALUES ('1', 'Ivan', 'ivan@example.com', true), ('2', 'Marta', 'marta@example.com', false), ('3', 'Javier', 'javier@example.com', false);");
        var sessionFactory = configuration.BuildSessionFactory();
        sessionFactory.Statistics.IsStatisticsEnabled = true;
        await using IUnitOfWork unitOfWork = new UnitOfWork(sessionFactory);
        await unitOfWork.BeginTransactionAsync();

        var filter = new OrFilter(
            new EqualsFilter(nameof(User.Username), "Nobody"),
            new AndFilter(
                new EqualsFilter(nameof(User.IsActive), false),
                new NotEqualsFilter(nameof(User.Username), "Marta"),
                new InFilter(nameof(User.Username), "Marta", "Javier"),
                new BetweenFilter(nameof(User.Username), "Ana", "Zoe"),
                new GreaterThanFilter(nameof(User.Username), "Ana"),
                new LessThanFilter(nameof(User.Username), "Zoe"),
                new LikeFilter(nameof(User.Username), "%v%"),
                new NotFilter(new EqualsFilter(nameof(User.Username), "Ivan"))));

        // Act
        await unitOfWork.DeleteAsync<User>(filter);

        // Assert
        Assert.Equal(0, sessionFactory.Statistics.EntityLoadCount);
        Assert.Equal(3, await npgsqlClient.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Users;"));

        await unitOfWork.CommitTransactionAsync();
        Assert.Equal(2, await npgsqlClient.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Users;"));
        Assert.Equal(0, await npgsqlClient.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Users WHERE name = 'Javier';"));
    }

    [Fact]
    public async Task Transaction()
    {
        // Arrange
        await using IUnitOfWork unitOfWork = new UnitOfWork(configuration.BuildSessionFactory());

        Assert.False(unitOfWork.IsTransactionActive);

        await unitOfWork.BeginTransactionAsync();
        Assert.True(unitOfWork.IsTransactionActive);
        var repository = unitOfWork.GetRepository<UserRepository>();

        // Act & Assert
        Assert.False(await unitOfWork.HasChangesAsync(default));

        var user = new User { Username = "Ivan", EmailAddress = "ivan@example.com", IsActive = true };
        await repository.AddAsync(user, default);

        Assert.True(await unitOfWork.HasChangesAsync(default));
        Assert.True(await unitOfWork.IsTrackedAsync(user, default));

        var shouldNull = await npgsqlClient.QuerySingleOrDefaultAsync<(string, string, string, bool)?>("SELECT * FROM Users;");
        Assert.Null(shouldNull);

        await unitOfWork.SaveChangesAsync(default);
        Assert.False(await unitOfWork.HasChangesAsync(default));
        var shouldNullBecauseUncommitTsx = await npgsqlClient.QuerySingleOrDefaultAsync<(string, string, string, bool)?>("SELECT * FROM Users;");
        Assert.Null(shouldNullBecauseUncommitTsx);

        await unitOfWork.CommitTransactionAsync();
        Assert.False(unitOfWork.IsTransactionActive);
        var shouldSaved = await npgsqlClient.QuerySingleOrDefaultAsync<(string, string, string, bool)?>("SELECT * FROM Users;");
        Assert.Equal("Ivan", shouldSaved!.Value.Item2);

        Assert.True(await unitOfWork.IsTrackedAsync(user, default));
        user.EmailAddress = "ivan2@example.com";
        await unitOfWork.SaveChangesAsync(default); // Outside tsx
        var shouldChanged = await npgsqlClient.QuerySingleOrDefaultAsync<(string, string, string, bool)?>("SELECT * FROM Users;");
        Assert.Equal("ivan2@example.com", shouldChanged!.Value.Item3);

        await Assert.ThrowsAsync<InvalidOperationException>(async () => await unitOfWork.CommitTransactionAsync());
    }

    [Fact]
    public async Task Rollback()
    {
        // Arrange
        await using IUnitOfWork unitOfWork = new UnitOfWork(configuration.BuildSessionFactory());

        var repository = unitOfWork.GetRepository<UserRepository>();

        var user = new User { Username = "Ivan", EmailAddress = "ivan@example.com", IsActive = true };
        await repository.AddAsync(user, default);
        await unitOfWork.SaveChangesAsync(default);

        await unitOfWork.BeginTransactionAsync();
        user.EmailAddress = "ivan2@example.com";
        await unitOfWork.SaveChangesAsync(default);
        await unitOfWork.RollbackTransactionAsync();
        var userSaved = await npgsqlClient.QuerySingleOrDefaultAsync<(string, string, string, bool)>("SELECT * FROM Users;");

        await Assert.ThrowsAsync<UnitOfWorkRolledBackException>(async () => { await unitOfWork.HasChangesAsync(default); });
        Assert.Equal("ivan2@example.com", user.EmailAddress);
        Assert.Equal("ivan@example.com", userSaved.Item3);
    }

    [Fact]
    public async Task StopTracking()
    {
        // Arrange
        await using IUnitOfWork unitOfWork = new UnitOfWork(configuration.BuildSessionFactory());

        var repository = unitOfWork.GetRepository<UserRepository>();
        var user = new User { Username = "Ivan", EmailAddress = "ivan@example.com", IsActive = true };

        // Act and Assert
        await repository.AddAsync(user, default);
        Assert.True(await unitOfWork.IsTrackedAsync(user, default));
        await unitOfWork.StopTrackingAsync(user, default);
        Assert.False(await unitOfWork.IsTrackedAsync(user, default));
    }

    [Fact]
    public async Task TransactionsRawProjections()
    {
        // Arrange
        await npgsqlClient.ExecuteAsync("INSERT INTO users (id, name, email, is_active) VALUES ('1', 'Ivan', 'ivan@example.com', true);");
        await using IUnitOfWork unitOfWork = new UnitOfWork(configuration.BuildSessionFactory());

        // Act
        var user = await unitOfWork.GetEntitySet<User>().FirstAsync();
        await unitOfWork.BeginTransactionAsync();

        user.Username = "Marta";
        await unitOfWork.SaveChangesAsync(default);

        var martaDto = (await unitOfWork.RawProjection<TransactionRawProjectionDto>("select name from users;")).First();
        var ivanDto = (await npgsqlClient.QueryAsync<TransactionRawProjectionDto>("select name from users;")).First();

        // Assert
        Assert.Equal("Marta", martaDto.Name);
        Assert.Equal("Ivan", ivanDto.Name);

        // Arrange post Assert
        await unitOfWork.RollbackTransactionAsync();
    }

    [Fact]
    public async Task MultipleUnitOfWorks()
    {
        // todo Discuss the behaviour of having more than one UoW
        // todo and reasons to have more than one UoW.

        // Arrange
        await npgsqlClient.ExecuteAsync("INSERT INTO Users (id, name, email, is_active) VALUES ('1', 'Ivan', 'ivan@example.com', true);");

        await using IUnitOfWork unitOfWork1 = new UnitOfWork(configuration.BuildSessionFactory());
        await using IUnitOfWork unitOfWork2 = new UnitOfWork(configuration.BuildSessionFactory());
        var repository1 = unitOfWork1.GetRepository<UserRepository>();
        var repository2 = unitOfWork2.GetRepository<UserRepository>();

        // Act
        var user1 = await repository1.GetByIdAsync("1", default);
        var user2 = await repository2.GetByIdAsync("1", default);

        // Assert
        Assert.NotSame(user1, user2);
        // Assert.True(await unitOfWork1.IsTrackedAsync(user1, default));
        // Assert.False(await unitOfWork2.IsTrackedAsync(user1, default));
        // Assert.False(await unitOfWork2.IsTrackedAsync(user2));
        // Assert.True(await unitOfWork1.IsTrackedAsync(user2, default));
        //
        // Arrange post Assert
    }
}

public record TransactionRawProjectionDto
{
    public required string Name { get; init; }
}