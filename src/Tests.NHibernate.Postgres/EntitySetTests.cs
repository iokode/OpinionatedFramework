using System.Linq;
using System.Threading.Tasks;
using Dapper;
using IOKode.OpinionatedFramework.ContractImplementations.NHibernate;
using IOKode.OpinionatedFramework.Persistence.UnitOfWork;
using IOKode.OpinionatedFramework.Persistence.UnitOfWork.QueryBuilder.Exceptions;
using IOKode.OpinionatedFramework.Tests.NHibernate.Postgres.Config;
using Xunit;

namespace IOKode.OpinionatedFramework.Tests.NHibernate.Postgres;

[Collection(nameof(NHibernateTestsFixtureCollection))]
public class EntitySetTests(NHibernateTestsFixture fixture) : NHibernateTestsBase(fixture)
{
    [Fact]
    public async Task GetById_Success()
    {
        // Arrange
        var sessionFactory = configuration.BuildSessionFactory();
        await using IUnitOfWork unitOfWork = new UnitOfWork(sessionFactory);

        await CreateUsersTableQueryAsync();
        await npgsqlClient.ExecuteAsync("INSERT INTO Users (id, name, email, is_active) VALUES ('1', 'Ivan', 'ivan@example.com', true);");
        await npgsqlClient.ExecuteAsync("INSERT INTO Users (id, name, email, is_active) VALUES ('2', 'Marta', 'marta@example.com', false);");
        await npgsqlClient.ExecuteAsync("INSERT INTO Users (id, name, email, is_active) VALUES ('3', 'Javier', 'javier@example.com', false);");
        var repository = unitOfWork.GetRepository<UserRepository>();

        // Act and Assert
        var user1 = await repository.GetByIdAsync("1", default);
        var user2 = await repository.GetByIdAsync("2", default);
        var user3 = await repository.GetByIdOrDefaultAsync("3", default);
        var userNull = await repository.GetByIdOrDefaultAsync("5", default);
        
        Assert.Equal("Ivan", user1.Username);
        Assert.Equal("ivan@example.com", user1.EmailAddress);
        Assert.True(user1.IsActive);
        Assert.Equal("Marta", user2.Username);
        Assert.Equal("marta@example.com", user2.EmailAddress);
        Assert.False(user2.IsActive);
        Assert.Equal("Javier", user3.Username);
        Assert.Equal("javier@example.com", user3.EmailAddress);
        Assert.False(user3.IsActive);
        Assert.Null(userNull);
        await Assert.ThrowsAsync<EntityNotFoundException>(async () =>
        {
            await repository.GetByIdAsync("4", default);
        });

        // Arrange post Assert
        await DropUsersTableQueryAsync();
    }
    
    [Fact]
    public async Task Single_Success()
    {
        // Arrange
        var sessionFactory = configuration.BuildSessionFactory();
        await using IUnitOfWork unitOfWork = new UnitOfWork(sessionFactory);

        await CreateUsersTableQueryAsync();
        await npgsqlClient.ExecuteAsync("INSERT INTO Users (id, name, email, is_active) VALUES ('1', 'Ivan', 'ivan@example.com', true);");
        var repository = unitOfWork.GetRepository<UserRepository>();

        // Act
        var user = await repository.GetByUsernameAsync("Ivan", default);

        // Assert
        Assert.Equal("Ivan", user.Username);
        Assert.Equal("ivan@example.com", user.EmailAddress);
        Assert.True(user.IsActive);

        // Arrange post Assert
        await DropUsersTableQueryAsync();
    }

    [Fact]
    public async Task Single_Fail()
    {
        // Arrange
        var sessionFactory = configuration.BuildSessionFactory();
        await using IUnitOfWork unitOfWork = new UnitOfWork(sessionFactory);

        await CreateUsersTableQueryAsync();
        await npgsqlClient.ExecuteAsync("INSERT INTO Users (id, name, email, is_active) VALUES ('1', 'Ivan', 'ivan@example.com', true);");
        await npgsqlClient.ExecuteAsync("INSERT INTO Users (id, name, email, is_active) VALUES ('2', 'Ivan', 'ivan@example.com', true);");
        var repository = unitOfWork.GetRepository<UserRepository>();

        // Act and Assert
        await Assert.ThrowsAsync<NonUniqueResultException>(async () => { await repository.GetByUsernameAsync("Ivan", default); });
        await Assert.ThrowsAsync<EmptyResultException>(async () => { await repository.GetByUsernameAsync("Marta", default); });

        // Arrange post Assert
        await DropUsersTableQueryAsync();
    }

    [Fact]
    public async Task SingleOrDefault_Success()
    {
        // Arrange
        var sessionFactory = configuration.BuildSessionFactory();
        await using IUnitOfWork unitOfWork = new UnitOfWork(sessionFactory);

        await CreateUsersTableQueryAsync();
        await npgsqlClient.ExecuteAsync("INSERT INTO Users (id, name, email, is_active) VALUES ('1', 'Ivan', 'ivan@example.com', true);");

        // Act
        var repository = unitOfWork.GetRepository<UserRepository>();
        var user = await repository.GetByEmailAddressOrDefaultAsync("ivan@example.com", default);
        var userNull = await repository.GetByEmailAddressOrDefaultAsync("ivan@example.net", default);

        // Assert
        Assert.Equal("Ivan", user.Username);
        Assert.Equal("ivan@example.com", user.EmailAddress);
        Assert.True(user.IsActive);
        Assert.Null(userNull);

        // Arrange post Assert
        await DropUsersTableQueryAsync();
    }

    [Fact]
    public async Task SingleOrDefault_Fail()
    {
        // Arrange
        var sessionFactory = configuration.BuildSessionFactory();
        await using IUnitOfWork unitOfWork = new UnitOfWork(sessionFactory);

        await CreateUsersTableQueryAsync();
        await npgsqlClient.ExecuteAsync("INSERT INTO Users (id, name, email, is_active) VALUES ('1', 'Ivan', 'ivan@example.com', true);");
        await npgsqlClient.ExecuteAsync("INSERT INTO Users (id, name, email, is_active) VALUES ('2', 'Ivan', 'ivan@example.com', true);");
        var repository = unitOfWork.GetRepository<UserRepository>();

        // Act and Assert
        var user = await repository.GetByEmailAddressOrDefaultAsync("ivan@example.net", default);
        Assert.Null(user);

        await Assert.ThrowsAsync<NonUniqueResultException>(async () => { await repository.GetByEmailAddressOrDefaultAsync("ivan@example.com", default); });

        // Arrange post Assert
        await DropUsersTableQueryAsync();
    }

    [Fact]
    public async Task First_Success()
    {
        // Arrange
        var sessionFactory = configuration.BuildSessionFactory();
        await using IUnitOfWork unitOfWork = new UnitOfWork(sessionFactory);

        await CreateUsersTableQueryAsync();
        await npgsqlClient.ExecuteAsync("INSERT INTO Users (id, name, email, is_active) VALUES ('1', 'Ivan', 'ivan@example.com', true);");
        await npgsqlClient.ExecuteAsync("INSERT INTO Users (id, name, email, is_active) VALUES ('2', 'Marta', 'marta@example.com', true);");
        var repository = unitOfWork.GetRepository<UserRepository>();

        // Act
        var user = await repository.GetFirstActiveAsync(default);

        // Assert
        Assert.Equal("Ivan", user.Username);
        Assert.Equal("ivan@example.com", user.EmailAddress);
        Assert.True(user.IsActive);

        // Arrange post Assert
        await DropUsersTableQueryAsync();
    }

    [Fact]
    public async Task First_Fail()
    {
        // Arrange
        var sessionFactory = configuration.BuildSessionFactory();
        await using IUnitOfWork unitOfWork = new UnitOfWork(sessionFactory);

        await CreateUsersTableQueryAsync();
        await npgsqlClient.ExecuteAsync("INSERT INTO Users (id, name, email, is_active) VALUES ('1', 'Ivan', 'ivan@example.com', false);");
        var repository = unitOfWork.GetRepository<UserRepository>();

        // Act and Assert
        await Assert.ThrowsAsync<EmptyResultException>(async () => { await repository.GetFirstActiveAsync(default); });

        // Arrange post Assert
        await DropUsersTableQueryAsync();
    }

    [Fact]
    public async Task FirstOrDefault_Success()
    {
        // Arrange
        var sessionFactory = configuration.BuildSessionFactory();
        await using IUnitOfWork unitOfWork = new UnitOfWork(sessionFactory);

        await CreateUsersTableQueryAsync();
        await npgsqlClient.ExecuteAsync("INSERT INTO Users (id, name, email, is_active) VALUES ('1', 'Ivan', 'ivan@example.com', true);");
        await npgsqlClient.ExecuteAsync("INSERT INTO Users (id, name, email, is_active) VALUES ('2', 'Ivan', 'ivan2@example.com', false);");
        var repository = unitOfWork.GetRepository<UserRepository>();

        // Act
        var user = await repository.GetFirstByNameOrDefaultAsync("Ivan", default);
        var userNull = await repository.GetFirstByNameOrDefaultAsync("Marta", default);

        // Assert
        Assert.Equal("Ivan", user!.Username);
        Assert.Equal("ivan@example.com", user.EmailAddress);
        Assert.True(user.IsActive);
        Assert.Null(userNull);

        // Arrange post Assert
        await DropUsersTableQueryAsync();
    }

    [Fact]
    public async Task Many_Success()
    {
        // Arrange
        var sessionFactory = configuration.BuildSessionFactory();
        await using IUnitOfWork unitOfWork = new UnitOfWork(sessionFactory);

        await CreateUsersTableQueryAsync();
        await npgsqlClient.ExecuteAsync("INSERT INTO Users (id, name, email, is_active) VALUES ('1', 'Ivan', 'ivan@example.com', true);");
        await npgsqlClient.ExecuteAsync("INSERT INTO Users (id, name, email, is_active) VALUES ('2', 'Marta', 'marta@example.com', false);");
        var repository = unitOfWork.GetRepository<UserRepository>();

        // Act
        var users = await repository.GetMultipleByNameAsync(["Ivan", "Marta"], default);
        var usersEmpty = await repository.GetMultipleByNameAsync(["Javier"], default);

        // Assert
        Assert.Equal(2, users.Count);
        Assert.Equal("Ivan", users.ToArray()[0].Username);
        Assert.Equal("Marta", users.ToArray()[1].Username);
        Assert.Empty(usersEmpty);

        // Arrange post Assert
        await DropUsersTableQueryAsync();
    }

    [Fact]
    public async Task GetAll()
    {
        // Arrange
        var sessionFactory = configuration.BuildSessionFactory();
        await using IUnitOfWork unitOfWork = new UnitOfWork(sessionFactory);

        await CreateUsersTableQueryAsync();
        await npgsqlClient.ExecuteAsync("INSERT INTO Users (id, name, email, is_active) VALUES ('1', 'Ivan', 'ivan@example.com', true);");
        await npgsqlClient.ExecuteAsync("INSERT INTO Users (id, name, email, is_active) VALUES ('2', 'Marta', 'marta@example.com', false);");
        var repository = unitOfWork.GetRepository<UserRepository>();

        // Act
        var users = await repository.GetAllAsync(default);

        // Assert
        Assert.Equal(2, users.Count);
        Assert.Equal("Ivan", users.ToArray()[0].Username);
        Assert.Equal("Marta", users.ToArray()[1].Username);

        // Arrange post Assert
        await DropUsersTableQueryAsync();
    }
}