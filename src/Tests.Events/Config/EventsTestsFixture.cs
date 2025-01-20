using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Docker.DotNet;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using Hangfire;
using Hangfire.PostgreSql;
using IOKode.OpinionatedFramework.Bootstrapping;
using IOKode.OpinionatedFramework.ContractImplementations.Hangfire.Jobs;
using IOKode.OpinionatedFramework.ContractImplementations.NHibernate;
using IOKode.OpinionatedFramework.ContractImplementations.NHibernate.Npgsql;
using IOKode.OpinionatedFramework.Events;
using IOKode.OpinionatedFramework.Tests.Helpers.Containers;
using Npgsql;
using Xunit;

namespace IOKode.OpinionatedFramework.Tests.Events.Config;

public class EventsTestsFixture : IAsyncLifetime
{
    private DockerClient docker => DockerHelper.DockerClient;

    public readonly PostgresContainer PostgresContainer = new();
    public NpgsqlConnection? NpgsqlClient = null;
    public BackgroundJobServer? HangfireServer = null;

    public async Task InitializeAsync()
    {
        await PostgresContainer.InitializeAsync();
        string postgresConnectionString = PostgresHelper.DefaultConnectionString;

        Container.Services.AddNHibernate(cfg =>
        {
            Fluently.Configure(cfg)
                .Database(PostgreSQLConfiguration.PostgreSQL83
                    .ConnectionString(postgresConnectionString))
                .Mappings(mapCfg =>
                {
                    mapCfg.FluentMappings.Add<EventMap>();
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
        Container.Services.AddHangfireJobsImplementations(cfg =>
        {
            cfg.UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(cfgPostgres => cfgPostgres.UseNpgsqlConnection(postgresConnectionString));
        });
        Container.Initialize();

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