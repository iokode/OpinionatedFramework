using System.Threading.Tasks;
using Dapper;
using IOKode.OpinionatedFramework.ContractImplementations.NHibernate;
using IOKode.OpinionatedFramework.Persistence.UnitOfWork;
using Xunit;
using Xunit.Abstractions;

namespace IOKode.OpinionatedFramework.Tests.NHibernate;

public class UnitOfWorkTests(ITestOutputHelper output) : NHibernateTestsBase(output)
{
    [Fact]
    public async Task IdentityMap_SameInstance()
    {
        // Arrange
        await _npgsqlClient.ExecuteAsync("CREATE TABLE IF NOT EXISTS Users (id TEXT PRIMARY KEY, name VARCHAR(100) NOT NULL, email VARCHAR(100) NOT NULL, is_active BOOLEAN NOT NULL);");
        await _npgsqlClient.ExecuteAsync("INSERT INTO Users (id, name, email, is_active) VALUES ('1', 'Ivan', 'ivan@example.com', true);");
        IUnitOfWork unitOfWork = new UnitOfWork(_configuration.BuildSessionFactory());
        var repository = unitOfWork.GetRepository<UserRepository>();

        // Act
        var user1 = await repository.GetByUsername("Ivan");
        var user2 = await repository.GetByEmailAddress("ivan@example.com");

        // Assert
        Assert.Same(user1, user2);
    }
}