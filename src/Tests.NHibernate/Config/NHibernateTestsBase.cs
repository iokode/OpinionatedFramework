using System.Threading.Tasks;
using Dapper;
using Npgsql;

namespace IOKode.OpinionatedFramework.Tests.NHibernate.Config;

public abstract class NHibernateTestsBase(NHibernateTestsFixture fixture)
{
    protected NpgsqlConnection npgsqlClient => fixture.NpgsqlClient;
    protected global::NHibernate.Cfg.Configuration configuration => fixture.Configuration;

    protected async Task CreateUsersTableQueryAsync()
    {
        await npgsqlClient.ExecuteAsync("CREATE TABLE IF NOT EXISTS Users (id TEXT PRIMARY KEY, name VARCHAR(100) NOT NULL, email VARCHAR(100) NOT NULL, is_active BOOLEAN NOT NULL);");
    }

    protected async Task DropUsersTableQueryAsync()
    {
        await npgsqlClient.ExecuteAsync("DROP TABLE Users;");
    }
}