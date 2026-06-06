using System.Threading.Tasks;
using IOKode.OpinionatedFramework.TestHelpers.Configuration;
using IOKode.OpinionatedFramework.TestHelpers.Containers;
using Npgsql;
using Xunit;

namespace IOKode.OpinionatedFramework.Tests.Helpers;

public class PostgresContainerTests
{
    [Fact]
    public async Task WaitUntilPostgresServerIsReady_UsesConnectionReturnedByConfiguredFactory()
    {
        // Arrange
        await using var dataSource = new NpgsqlDataSourceBuilder(PostgresHelper.ConnectionString)
            .MapComposite<ReadinessAddress>("readiness_address_type")
            .Build();

        var docker = DockerHelper.DockerClient;
        var options = PostgresOptions.Default;
        options.OpenConnectionAsync = async cancellationToken => await dataSource.OpenConnectionAsync(cancellationToken);
        string? containerId = null;

        try
        {
            // Act
            //
            // The readiness check opens a connection through the configured NpgsqlDataSource,
            // and the same data source can then materialize a PostgreSQL composite type using
            // the mapping configured on its builder.
            await PostgresHelper.PullPostgresImage(docker, options);
            containerId = await PostgresHelper.RunPostgresContainer(docker, options);
            await PostgresHelper.WaitUntilPostgresServerIsReady(docker, containerId, options);

            await using var setupConnection = await dataSource.OpenConnectionAsync();
            await using var setupCommand = setupConnection.CreateCommand();
            setupCommand.CommandText = """
                                      DROP TYPE IF EXISTS readiness_address_type;
                                      CREATE TYPE readiness_address_type AS (line TEXT, region TEXT, country_code TEXT);
                                      """;
            await setupCommand.ExecuteNonQueryAsync();
            await dataSource.ReloadTypesAsync();

            await using var command = dataSource.CreateCommand("SELECT ROW('Fake St. 123', 'Springfield', 'USA')::readiness_address_type;");
            await using var reader = await command.ExecuteReaderAsync();
            await reader.ReadAsync();
            var address = await reader.GetFieldValueAsync<ReadinessAddress>(0);

            Assert.Equal("Fake St. 123", address.Line);
            Assert.Equal("Springfield", address.Region);
            Assert.Equal("USA", address.CountryCode);
        }
        finally
        {
            if (containerId != null)
            {
                await DockerHelper.RemoveContainer(docker, containerId);
            }
        }
    }

    [Fact]
    public async Task InitializeAsync_UsesConfiguredContainerOptions()
    {
        // Arrange
        const string containerName = "oftest_postgres_options";
        const string hostPort = "15432";
        var connectionStringBuilder = new NpgsqlConnectionStringBuilder(PostgresHelper.ConnectionString)
        {
            Port = int.Parse(hostPort)
        };

        var docker = DockerHelper.DockerClient;
        var options = PostgresOptions.Default;
        options.Image = "postgres";
        options.Tag = "17";
        options.ContainerName = containerName;
        options.HostPort = hostPort;
        options.OpenConnectionAsync = async cancellationToken =>
        {
            var connection = new NpgsqlConnection(connectionStringBuilder.ConnectionString);
            await connection.OpenAsync(cancellationToken);
            return connection;
        };
        string? containerId = null;

        try
        {
            // Act
            await PostgresHelper.PullPostgresImage(docker, options);
            containerId = await PostgresHelper.RunPostgresContainer(docker, options);
            await PostgresHelper.WaitUntilPostgresServerIsReady(docker, containerId, options);

            // Assert
            var containerInspect = await docker.Containers.InspectContainerAsync(containerId);
            Assert.Equal($"/{containerName}", containerInspect.Name);
            Assert.Equal("postgres:17", containerInspect.Config.Image);
            Assert.Equal(hostPort, containerInspect.NetworkSettings.Ports["5432/tcp"][0].HostPort);
        }
        finally
        {
            if (containerId != null)
            {
                await DockerHelper.RemoveContainer(docker, containerId);
            }
        }
    }
}

public sealed class ReadinessAddress
{
    public string Line { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
}
