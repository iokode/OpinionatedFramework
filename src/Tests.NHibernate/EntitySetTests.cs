using System.Threading.Tasks;
using Dapper;
using IOKode.OpinionatedFramework.ContractImplementations.NHibernate;
using IOKode.OpinionatedFramework.Persistence.QueryBuilder.Exceptions;
using IOKode.OpinionatedFramework.Persistence.UnitOfWork;
using Xunit;
using Xunit.Abstractions;

namespace IOKode.OpinionatedFramework.Tests.NHibernate;

public class EntitySetTests(ITestOutputHelper output) : NHibernateTestsBase(output)
{
    [Fact]
    public async Task EntitySet_Single_Success()
    {
        // Arrange
        var sessionFactory = _configuration.BuildSessionFactory();
        IUnitOfWork unitOfWork = new UnitOfWork(sessionFactory);

        await _npgsqlClient.ExecuteAsync("CREATE TABLE IF NOT EXISTS Users (id TEXT PRIMARY KEY, name VARCHAR(100) NOT NULL, email VARCHAR(100) NOT NULL, is_active BOOLEAN NOT NULL);");
        await _npgsqlClient.ExecuteAsync("INSERT INTO Users (id, name, email, is_active) VALUES ('1', 'Ivan', 'testing@example.com', true);");

        // Act
        var repo = unitOfWork.GetRepository<UserRepository>();
        var user = await repo.GetByUsername("Ivan", default);

        // Assert
        Assert.Equal("Ivan", user.Username);
        Assert.Equal("testing@example.com", user.EmailAddress);
        Assert.True(user.IsActive);
        
        await _npgsqlClient.ExecuteAsync("DROP TABLE Users;");

        // user.IsActive = false;
        // await repo.AddAsync(new User
        // {
        //     Username = "Ivan",
        //     EmailAddress = "testing@example.com",
        //     IsActive = true
        // }, default);
        // await unitOfWork.SaveChangesAsync(default);
    }

    [Fact]
    public async Task EntitySet_Single_Fail()
    {
        // Arrange
        var sessionFactory = _configuration.BuildSessionFactory();
        IUnitOfWork unitOfWork = new UnitOfWork(sessionFactory);

        await _npgsqlClient.ExecuteAsync("CREATE TABLE IF NOT EXISTS Users (id TEXT PRIMARY KEY, name VARCHAR(100) NOT NULL, email VARCHAR(100) NOT NULL, is_active BOOLEAN NOT NULL);");
        await _npgsqlClient.ExecuteAsync("INSERT INTO Users (id, name, email, is_active) VALUES ('1', 'Ivan', 'testing@example.com', true);");
        await _npgsqlClient.ExecuteAsync("INSERT INTO Users (id, name, email, is_active) VALUES ('2', 'Ivan', 'testing@example.com', true);");
        var repo = unitOfWork.GetRepository<UserRepository>();

        // Act and Assert
        await Assert.ThrowsAsync<NonUniqueResultException>(async () =>
        {
            await repo.GetByUsername("Ivan", default);
        });
        await Assert.ThrowsAsync<EmptyResultException>(async () =>
        {
            await repo.GetByUsername("Marta", default);
        });
        
        await _npgsqlClient.ExecuteAsync("DROP TABLE Users;");
    }
    
    [Fact]
    public async Task EntitySet_SingleOrDefault_Success()
    {
        // Arrange
        var sessionFactory = _configuration.BuildSessionFactory();
        IUnitOfWork unitOfWork = new UnitOfWork(sessionFactory);

        await _npgsqlClient.ExecuteAsync("CREATE TABLE IF NOT EXISTS Users (id TEXT PRIMARY KEY, name VARCHAR(100) NOT NULL, email VARCHAR(100) NOT NULL, is_active BOOLEAN NOT NULL);");
        await _npgsqlClient.ExecuteAsync("INSERT INTO Users (id, name, email, is_active) VALUES ('1', 'Ivan', 'testing@example.com', true);");

        // Act
        var repo = unitOfWork.GetRepository<UserRepository>();
        var user = await repo.GetByEmailAddress("testing@example.com", default);
        var userNull = await repo.GetByEmailAddress("testing@example.net", default);

        // Assert
        Assert.Equal("Ivan", user.Username);
        Assert.Equal("testing@example.com", user.EmailAddress);
        Assert.True(user.IsActive);
        Assert.Null(userNull);
        
        await _npgsqlClient.ExecuteAsync("DROP TABLE Users;");
    }
    
    [Fact]
    public async Task EntitySet_SingleOrDefault_Fail()
    {
        // Arrange
        var sessionFactory = _configuration.BuildSessionFactory();
        IUnitOfWork unitOfWork = new UnitOfWork(sessionFactory);

        await _npgsqlClient.ExecuteAsync("CREATE TABLE IF NOT EXISTS Users (id TEXT PRIMARY KEY, name VARCHAR(100) NOT NULL, email VARCHAR(100) NOT NULL, is_active BOOLEAN NOT NULL);");
        await _npgsqlClient.ExecuteAsync("INSERT INTO Users (id, name, email, is_active) VALUES ('1', 'Ivan', 'testing@example.com', true);");
        await _npgsqlClient.ExecuteAsync("INSERT INTO Users (id, name, email, is_active) VALUES ('2', 'Ivan', 'testing@example.com', true);");
        var repo = unitOfWork.GetRepository<UserRepository>();

        // Act and Assert
        var user = await repo.GetByEmailAddress("testing@example.net", default);
        Assert.Null(user);

        await Assert.ThrowsAsync<NonUniqueResultException>(async () =>
        {
            await repo.GetByEmailAddress("testing@example.com", default);
        });

        await _npgsqlClient.ExecuteAsync("DROP TABLE Users;");
    }
    
    [Fact]
    public async Task EntitySet_First_Success()
    {
        
    }
    
    [Fact]
    public async Task EntitySet_First_Fail()
    {
        
    }
    
    [Fact]
    public async Task EntitySet_FirstOrDefault_Success()
    {
        
    }
    
    [Fact]
    public async Task EntitySet_FirstOrDefault_Fail()
    {
        
    }
    
    [Fact]
    public async Task EntitySet_Many_Success()
    {
        
    }
}