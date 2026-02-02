using System;
using System.Threading.Tasks;
using Dapper;
using Npgsql;
using Xunit;
using Xunit.Abstractions;

namespace IOKode.OpinionatedFramework.Tests.NHibernate.Postgres.Config;

public abstract class NHibernateTestsBase : IAsyncLifetime
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
        await npgsqlClient.ExecuteAsync("CREATE TYPE address_type AS (line VARCHAR(100), region VARCHAR(100), country_code VARCHAR(3));");
        await npgsqlClient.ReloadTypesAsync();
        await npgsqlClient.ExecuteAsync("CREATE TABLE IF NOT EXISTS Users (id TEXT PRIMARY KEY, name VARCHAR(100), email VARCHAR(100), is_active BOOLEAN, country_code VARCHAR(3), address address_type);");
    }

    protected async Task DropUsersTableQueryAsync()
    {
        await npgsqlClient.ExecuteAsync("DROP TABLE Users;");
        await npgsqlClient.ExecuteAsync("DROP TYPE address_type;");
    }

    public async Task InitializeAsync()
    {
        await CreateUsersTableQueryAsync();
    }

    public async Task DisposeAsync()
    {
        await DropUsersTableQueryAsync();
    }
}