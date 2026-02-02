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
using IOKode.OpinionatedFramework.Tests.NHibernate.Postgres.Config.Entities;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Xunit;
using Xunit.Abstractions;

namespace IOKode.OpinionatedFramework.Tests.NHibernate.Postgres.Config;

public class NHibernateTestsFixture : IAsyncLifetime
{
    private DockerClient docker => DockerHelper.DockerClient;

    public readonly PostgresContainer PostgresContainer = new();
    public NpgsqlConnection? NpgsqlClient;
    public global::NHibernate.Cfg.Configuration Configuration = null!;
    
    public async Task InitializeAsync()
    {
#pragma warning disable 0618
        NpgsqlConnection.GlobalTypeMapper.MapComposite<AddressDto>("public.address_type");
#pragma warning restore 0618
        Container.Services.AddTransient<ILogging>(_ => new XUnitLogging(TestOutputHelperFactory?.Invoke() ?? throw new NullReferenceException("TestOutputHelperFactory is null. Did you forget to set it in the constructor?")));
        Container.Services.AddScoped<ScopedService>();
        await PostgresContainer.InitializeAsync();
        string connectionString = PostgresHelper.ConnectionString;
        
        NpgsqlClient = new NpgsqlConnection(connectionString);
        await NpgsqlClient.OpenAsync();
        Container.Services.AddNHibernateWithPostgres(cfg =>
        {
            Configuration = cfg;
            cfg.AddXmlFile("Config/Entities/user.hbm.xml");
            Fluently.Configure(cfg)
                .Database(PostgreSQLConfiguration.PostgreSQL83
                    .ConnectionString(connectionString))
                .BuildConfiguration();
        });
        Container.Initialize();
        UserTypeMapper.AddUserType<Address, AddressType>();
        UserTypeMapper.AddUserType<CountryCode, CountryCodeType>();
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

    public Func<ITestOutputHelper>? TestOutputHelperFactory { get; set; }
}

[CollectionDefinition(nameof(NHibernateTestsFixtureCollection))]
public class NHibernateTestsFixtureCollection : ICollectionFixture<NHibernateTestsFixture>;