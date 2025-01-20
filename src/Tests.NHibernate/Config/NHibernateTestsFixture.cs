using System.Threading.Tasks;
using Docker.DotNet;
using IOKode.OpinionatedFramework.Tests.Helpers.Containers;
using Npgsql;
using Xunit;

namespace IOKode.OpinionatedFramework.Tests.NHibernate.Config;

public class NHibernateTestsFixture : IAsyncLifetime
{
    private DockerClient docker => DockerHelper.DockerClient;

    public readonly PostgresContainer PostgresContainer = new();
    public NpgsqlConnection? NpgsqlClient = null;
    public global::NHibernate.Cfg.Configuration Configuration = null!;
    
    public async Task InitializeAsync()
    {
        await PostgresContainer.InitializeAsync();
        string connectionString = PostgresHelper.DefaultConnectionString;
        
        Configuration = new global::NHibernate.Cfg.Configuration();
        Configuration.Properties.Add("connection.connection_string", connectionString);
        Configuration.Properties.Add("dialect", "NHibernate.Dialect.PostgreSQL83Dialect");
        Configuration.AddXmlFile("Config/user.hbm.xml");

        NpgsqlClient = new NpgsqlConnection(connectionString);
        await NpgsqlClient.OpenAsync();
    }

    public async Task DisposeAsync()
    {
        if (NpgsqlClient != null)
        {
            await NpgsqlClient.CloseAsync();
        }

        await PostgresContainer.DisposeAsync();
        
        docker.Dispose();
    }
}

[CollectionDefinition(nameof(NHibernateTestsFixtureCollection))]
public class NHibernateTestsFixtureCollection : ICollectionFixture<NHibernateTestsFixture>;