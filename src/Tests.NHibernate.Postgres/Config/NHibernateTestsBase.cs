using System;
using System.Threading.Tasks;
using Dapper;
using Npgsql;
using Xunit.Abstractions;

namespace IOKode.OpinionatedFramework.Tests.NHibernate.Postgres.Config;

public abstract class NHibernateTestsBase
{
    private readonly NHibernateTestsFixture fixture;

    public NHibernateTestsBase(NHibernateTestsFixture fixture, ITestOutputHelper outputHelper)
    {
        fixture.TestOutputHelperFactory = () => outputHelper;
        this.fixture = fixture;
    }
    
    protected NpgsqlConnection npgsqlClient => fixture.NpgsqlClient ?? throw new NullReferenceException("NpgsqlClient from fixture is null. Did you forget to await fixture.InitializeAsync()?.");
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