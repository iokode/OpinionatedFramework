using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using IOKode.OpinionatedFramework.ContractImplementations.NHibernate;
using IOKode.OpinionatedFramework.Persistence.UnitOfWork;
using IOKode.OpinionatedFramework.Persistence.UnitOfWork.Exceptions;
using IOKode.OpinionatedFramework.Tests.NHibernate.Config;
using Xunit;

namespace IOKode.OpinionatedFramework.Tests.NHibernate;

[Collection(nameof(NHibernateTestsFixtureCollection))]
public class UnitOfWorkTests(NHibernateTestsFixture fixture) : NHibernateTestsBase(fixture)
{
    [Fact]
    public async Task IdentityMap_SameInstance()
    {
        // Arrange
        await CreateUsersTableQueryAsync();
        await npgsqlClient.ExecuteAsync("INSERT INTO Users (id, name, email, is_active) VALUES ('1', 'Ivan', 'ivan@example.com', true);");
        await using IUnitOfWork unitOfWork = new UnitOfWork(configuration.BuildSessionFactory());
        var repository = unitOfWork.GetRepository<UserRepository>();

        // Act
        var user1 = await repository.GetByUsernameAsync("Ivan");
        var user2 = await repository.GetByEmailAddressOrDefaultAsync("ivan@example.com");

        // Assert
        Assert.Same(user1, user2);

        // Arrange post assert
        await DropUsersTableQueryAsync();
    }

    [Fact]
    public async Task EnsureQueriedEntitiesAreTracked()
    {
        // Arrange 
        await CreateUsersTableQueryAsync();
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

        // Arrange post assert
        await DropUsersTableQueryAsync();
    }

    [Fact]
    public async Task Add()
    {
        // Arrange
        await CreateUsersTableQueryAsync();
        await using IUnitOfWork unitOfWork = new UnitOfWork(configuration.BuildSessionFactory());
        var repository = unitOfWork.GetRepository<UserRepository>();

        // Act & Assert
        Assert.False(await unitOfWork.HasChangesAsync(default));

        var user = new User {Username = "Ivan", EmailAddress = "ivan@example.com", IsActive = true};
        await repository.AddAsync(user, default);

        Assert.True(await unitOfWork.HasChangesAsync(default));
        Assert.True(await unitOfWork.IsTrackedAsync(user, default));

        var shouldNull = await npgsqlClient.QuerySingleOrDefaultAsync<(string, string, string, bool)?>("SELECT * FROM Users;");
        Assert.Null(shouldNull);

        await unitOfWork.SaveChangesAsync(default);
        var shouldSaved = await npgsqlClient.QuerySingleOrDefaultAsync<(string, string, string, bool)>("SELECT * FROM Users;");
        Assert.Equal("Ivan", shouldSaved.Item2);
        await Assert.ThrowsAsync<ArgumentException>(async () => { await repository.AddAsync(user, default); });

        // Arrange post Assert
        await DropUsersTableQueryAsync();
    }

    [Fact]
    public async Task Transaction()
    {
        // Arrange
        await CreateUsersTableQueryAsync();
        await using IUnitOfWork unitOfWork = new UnitOfWork(configuration.BuildSessionFactory());

        Assert.False(unitOfWork.IsTransactionActive);

        await unitOfWork.BeginTransactionAsync();
        Assert.True(unitOfWork.IsTransactionActive);
        var repository = unitOfWork.GetRepository<UserRepository>();

        // Act & Assert
        Assert.False(await unitOfWork.HasChangesAsync(default));

        var user = new User {Username = "Ivan", EmailAddress = "ivan@example.com", IsActive = true};
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

        // Arrange post Assert
        await DropUsersTableQueryAsync();
    }

    [Fact]
    public async Task Rollback()
    {
        // Arrange
        await CreateUsersTableQueryAsync();
        await using IUnitOfWork unitOfWork = new UnitOfWork(configuration.BuildSessionFactory());

        var repository = unitOfWork.GetRepository<UserRepository>();

        var user = new User {Username = "Ivan", EmailAddress = "ivan@example.com", IsActive = true};
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

        // Arrange post Assert
        await DropUsersTableQueryAsync();
    }

    [Fact]
    public async Task StopTracking()
    {
        // Arrange
        await CreateUsersTableQueryAsync();
        await using IUnitOfWork unitOfWork = new UnitOfWork(configuration.BuildSessionFactory());

        var repository = unitOfWork.GetRepository<UserRepository>();
        var user = new User {Username = "Ivan", EmailAddress = "ivan@example.com", IsActive = true};

        // Act and Assert
        await repository.AddAsync(user, default);
        Assert.True(await unitOfWork.IsTrackedAsync(user, default));
        await unitOfWork.StopTrackingAsync(user, default);
        Assert.False(await unitOfWork.IsTrackedAsync(user, default));

        // Arrange post Assert
        await DropUsersTableQueryAsync();
    }

    [Fact]
    public async Task TransactionsRawProjections()
    {
        // Arrange
        await CreateUsersTableQueryAsync();
        await npgsqlClient.ExecuteAsync("INSERT INTO Users (id, name, email, is_active) VALUES ('1', 'Ivan', 'ivan@example.com', true);");
        await using IUnitOfWork unitOfWork = new UnitOfWork(configuration.BuildSessionFactory());

        // Act
        var user = await unitOfWork.GetEntitySet<User>().FirstAsync();
        await unitOfWork.BeginTransactionAsync();

        user.Username = "Marta";
        await unitOfWork.SaveChangesAsync(default);

        string shouldBeMarta = (await unitOfWork.RawProjection<string>("select name from Users;")).First();
        string shouldBeIvan = (await npgsqlClient.QueryAsync<string>("select name from Users;")).First();

        // Assert
        Assert.Equal("Marta", shouldBeMarta);
        Assert.Equal("Ivan", shouldBeIvan);

        // Arrange post Assert
        await unitOfWork.RollbackTransactionAsync();
        await DropUsersTableQueryAsync();
    }

    [Fact]
    public async Task MultipleUnitOfWorks()
    {
        // todo Discuss the behaviour of having more than one UoW
        // todo and reasons to have more than one UoW.

        // Arrange
        await CreateUsersTableQueryAsync();
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
        await DropUsersTableQueryAsync();
    }
}