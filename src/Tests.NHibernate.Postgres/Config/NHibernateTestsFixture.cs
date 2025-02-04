using System;
using System.Threading.Tasks;
using Docker.DotNet;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using IOKode.OpinionatedFramework.Bootstrapping;
using IOKode.OpinionatedFramework.ContractImplementations.NHibernate;
using IOKode.OpinionatedFramework.ContractImplementations.NHibernate.Postgres;
using IOKode.OpinionatedFramework.Logging;
using IOKode.OpinionatedFramework.Tests.Helpers;
using IOKode.OpinionatedFramework.Tests.Helpers.Containers;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Xunit;
using Xunit.Abstractions;

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
        Container.Services.AddTransient<ILogging>(_ => new XUnitLogging(TestOutputHelperFactory()));
        await PostgresContainer.InitializeAsync();
        string connectionString = PostgresHelper.DefaultConnectionString;
        
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
    
    public Func<ITestOutputHelper> TestOutputHelperFactory { get; set; }
}

[CollectionDefinition(nameof(NHibernateTestsFixtureCollection))]
public class NHibernateTestsFixtureCollection : ICollectionFixture<NHibernateTestsFixture>;