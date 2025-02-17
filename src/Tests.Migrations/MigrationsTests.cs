using System.Reflection;
using System.Threading.Tasks;
using Dapper;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using IOKode.OpinionatedFramework.Bootstrapping;
using IOKode.OpinionatedFramework.Tests.Helpers.Containers;
using IOKode.OpinionatedFramework.Tests.Migrations.Config.MigrationFiles;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Xunit;

namespace IOKode.OpinionatedFramework.Tests.Migrations;

public class MigrationsTests(PostgresContainer fixture) : IClassFixture<PostgresContainer>, IAsyncLifetime
{
    private readonly NpgsqlConnection npgsqlClient = new(PostgresHelper.ConnectionString);

    [Fact]
    public void GeneratedMigrations()
    {
        // Arrange
        var assembly = Assembly.GetExecutingAssembly();
        const string migration1Name = "IOKode.OpinionatedFramework.Tests.Migrations.Config.MigrationFiles.CreateSchemaMigration";
        const string migration2Name = "MigrationFiles.SecondMigration";
        const string migration3Name = "IOKode.OpinionatedFramework.Tests.Migrations.Config.MigrationFiles.ThirdMigration";

        // Act
        var migration1Type = assembly.GetType(migration1Name);
        var migration2Type = assembly.GetType(migration2Name);
        var migration3Type = assembly.GetType(migration3Name);

        // Assert
        Assert.NotNull(migration1Type);
        Assert.NotNull(migration2Type);
        Assert.NotNull(migration3Type);
        Assert.Equal(migration1Name, migration1Type.FullName);
        Assert.Equal(migration2Name, migration2Type.FullName);
        Assert.Equal(migration3Name, migration3Type.FullName);
    }

    [Fact]
    public async Task Runner()
    {
        // Arrange
        var migrationRunner = Locator.Resolve<IMigrationRunner>();

        // Act and Assert
        migrationRunner.MigrateUp(3);
        await npgsqlClient.ExecuteAsync("INSERT INTO iokode.users (id, name) VALUES (1, 'Ivan');");
        await Assert.ThrowsAsync<PostgresException>(async () => await npgsqlClient.ExecuteAsync("INSERT INTO iokode.products (id, name) VALUES (1, 'Product1');"));

        migrationRunner.MigrateDown(1);
        await Assert.ThrowsAsync<PostgresException>(async () => await npgsqlClient.ExecuteAsync("INSERT INTO iokode.users (id, name) VALUES (2, 'Ivan');"));
    }

    public async Task InitializeAsync()
    {
        Container.Services.AddFluentMigratorCore()
            .ConfigureRunner(runnerBuilder => runnerBuilder
                .AddPostgres()
                .WithGlobalConnectionString(PostgresHelper.ConnectionString)
                .ScanIn(Assembly.GetAssembly(typeof(CreateSchemaMigration))!).For.Migrations()
            )
            .Configure<RunnerOptions>(cfg => { cfg.Tags = ["iokode"]; });
        Container.Initialize();
        await npgsqlClient.OpenAsync();
    }

    public async Task DisposeAsync()
    {
        await npgsqlClient.CloseAsync();
        Container.Advanced.Clear();
    }
}