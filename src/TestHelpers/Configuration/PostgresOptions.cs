using System;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.TestHelpers.Containers;
using Npgsql;

namespace IOKode.OpinionatedFramework.TestHelpers.Configuration;

public class PostgresOptions
{
    public required string Image { get; set; }

    public required string Tag { get; set; }

    public required string ContainerName { get; set; }

    public required string HostPort { get; set; }

    public string ImageWithTag => $"{Image}:{Tag}";

    /// <summary>
    /// Opens the connection used by the PostgreSQL readiness check.
    /// </summary>
    /// <remarks>
    /// Callers using <see cref="NpgsqlDataSource" /> can set this to open a connection from their
    /// configured data source, for example after configuring an <see cref="NpgsqlDataSourceBuilder" />
    /// with PostgreSQL composite mappings.
    /// </remarks>
    public required Func<CancellationToken, Task<NpgsqlConnection>> OpenConnectionAsync { get; set; }

    public static PostgresOptions Default = new()
    {
        Image = "postgres",
        Tag = "18",
        ContainerName = "oftest_nhibernate_postgres",
        HostPort = "5432",
        OpenConnectionAsync = async cancellationToken =>
        {
            var connection = new NpgsqlConnection(PostgresHelper.ConnectionString);
            await connection.OpenAsync(cancellationToken);
            return connection;
        }
    };
}
