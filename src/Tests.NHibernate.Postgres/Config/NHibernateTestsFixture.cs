using System.Threading.Tasks;
using Docker.DotNet;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using IOKode.OpinionatedFramework.Bootstrapping;
using IOKode.OpinionatedFramework.ContractImplementations.NHibernate;
using IOKode.OpinionatedFramework.ContractImplementations.NHibernate.Postgres;
using IOKode.OpinionatedFramework.Tests.Helpers.Containers;
using Npgsql;
using Xunit;

namespace IOKode.OpinionatedFramework.Tests.NHibernate.Postgres.Config;

public class NHibernateTestsFixture : IAsyncLifetime
{
    private DockerClient docker => DockerHelper.DockerClient;

    public readonly PostgresContainer PostgresContainer = new();
    public NpgsqlConnection? NpgsqlClient = null;
    public global::NHibernate.Cfg.Configuration Configuration = null!;
    
    public async Task InitializeAsync()
    {
        NpgsqlConnection.GlobalTypeMapper.MapComposite<AddressDto>("public.address_type");
        await PostgresContainer.InitializeAsync();
        string connectionString = PostgresHelper.ConnectionString;
        
        NpgsqlClient = new NpgsqlConnection(connectionString);
        await NpgsqlClient.OpenAsync();
        Container.Services.AddNHibernateWithPostgres(cfg =>
        {
            Configuration = cfg;
            cfg.AddXmlFile("Config/user.hbm.xml");
            Fluently.Configure(cfg)
                .Database(PostgreSQLConfiguration.PostgreSQL83
                    .ConnectionString(connectionString))
                .BuildConfiguration();
        });
        Container.Initialize();
        UserTypeMapper.AddUserType<Address, AddressType>();
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