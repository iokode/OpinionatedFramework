using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Docker.DotNet;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using Hangfire;
using Hangfire.PostgreSql;
using IOKode.OpinionatedFramework.Bootstrapping;
using IOKode.OpinionatedFramework.ContractImplementations.Hangfire.Jobs;
using IOKode.OpinionatedFramework.ContractImplementations.NHibernate.Postgres;
using IOKode.OpinionatedFramework.ContractImplementations.NHibernate.Postgres.Mappings;
using IOKode.OpinionatedFramework.ContractImplementations.NHibernate.Postgres.Migrations;
using IOKode.OpinionatedFramework.Events;
using IOKode.OpinionatedFramework.Tests.Helpers.Containers;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Xunit;

namespace IOKode.OpinionatedFramework.Tests.Events.Config;

public class EventsTestsFixture : IAsyncLifetime
{
    private DockerClient docker => DockerHelper.DockerClient;

    public readonly PostgresContainer PostgresContainer = new();
    public NpgsqlConnection? NpgsqlClient;
    public BackgroundJobServer? HangfireServer;

    public async Task InitializeAsync()
    {
        await PostgresContainer.InitializeAsync();
        string postgresConnectionString = PostgresHelper.ConnectionString;

        Container.Services.AddNHibernateWithPostgres(options =>
        {
            Fluently.Configure(options)
                .Database(PostgreSQLConfiguration.PostgreSQL83
                    .ConnectionString(postgresConnectionString))
                .Mappings(mapCfg =>
                {
                    mapCfg.FluentMappings.AddOpinionatedFrameworkPostgresMappings();
                    var assembly = Assembly.GetExecutingAssembly();
                    var eventSubclasses = assembly.GetTypes()
                        .Where(type => type.IsSubclassOf(typeof(Event)) && !type.IsAbstract);

                    foreach (var subclass in eventSubclasses)
                    {
                        var genericMapType = typeof(EventSubclassMap<>).MakeGenericType(subclass);
                        mapCfg.FluentMappings.Add(genericMapType);
                    }
                })
                .BuildConfiguration();
        });
        
        Container.Services.AddFluentMigratorCore()
            .ConfigureRunner(runnerBuilder =>
                runnerBuilder
                    .AddPostgres()
                    .WithGlobalConnectionString(postgresConnectionString)
                    .ScanIn(Assembly.GetAssembly(typeof(CreateSchemaMigration))!).For.Migrations()
            )
            .Configure<RunnerOptions>(cfg => cfg.Tags = ["opinionated_framework"]);
        
        GlobalConfiguration.Configuration.UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(cfgPostgres => cfgPostgres.UseNpgsqlConnection(postgresConnectionString));
        Container.Services.AddHangfireJobsImplementations();
        
        Container.Initialize();

        Locator.Resolve<IMigrationRunner>().MigrateUp();
        HangfireServer = new BackgroundJobServer(new BackgroundJobServerOptions
        {
            Queues = ["eventing"]
        });
        NpgsqlClient = new NpgsqlConnection(postgresConnectionString);
        await Task.Delay(3000);
    }

    public async Task DisposeAsync()
    {
        if (NpgsqlClient != null)
        {
            await NpgsqlClient.CloseAsync();
            NpgsqlClient.Dispose();
        }

        if (HangfireServer != null)
        {
            HangfireServer.SendStop();
            await HangfireServer.WaitForShutdownAsync(default);
            HangfireServer.Dispose();
        }

        await PostgresContainer.DisposeAsync();

        docker.Dispose();
    }
}