using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Dapper;
using Docker.DotNet;
using Docker.DotNet.Models;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using IOKode.OpinionatedFramework.Bootstrapping;
using IOKode.OpinionatedFramework.ContractImplementations.Hangfire.Jobs;
using IOKode.OpinionatedFramework.ContractImplementations.NHibernate;
using IOKode.OpinionatedFramework.Events;
using IOKode.OpinionatedFramework.Utilities;
using MongoDB.Bson;
using MongoDB.Driver;
using Npgsql;
using Xunit;
using Xunit.Abstractions;

namespace IOKode.OpinionatedFramework.Tests.Events;

public class EventsTestsBase(ITestOutputHelper output) : IAsyncLifetime
{
    protected string _postgresContainerId = null!;
    protected string _mongoContainerId = null!;
    protected DockerClient _docker = null!;

    protected NpgsqlConnection _npgsqlClient = null!;
    protected BackgroundJobServer _hangfireServer = null!;

    public async Task InitializeAsync()
    {
        string dbPostgresConnectionString = "Server=localhost; Database=testdb; User Id=iokode; Password=secret;";
        var dbOptions = new MongoOptions
        {
            Database = "IOKodeHangfireTests",
            ConnectionString = "mongodb://root:Secret_123@localhost:27017"
        };
        var storageOptions = new MongoStorageOptions
        {
            MigrationOptions = new MongoMigrationOptions
            {
                MigrationStrategy = new DropMongoMigrationStrategy()
            },
            CheckQueuedJobsStrategy = CheckQueuedJobsStrategy.TailNotificationsCollection
        };

        _docker = new DockerClientConfiguration().CreateClient();
        await pullPostgresImage();
        await pullMongoImage();
        await runPostgresContainer();
        await runMongoContainer();
        await waitUntilPostgresServerIsReady();
        await waitUntilMongoServerIsReady();
        Container.Services.AddNHibernate(cfg =>
        {
            Fluently.Configure(cfg)
                .Database(PostgreSQLConfiguration.PostgreSQL83
                    .ConnectionString(dbPostgresConnectionString))
                .Mappings(mapCfg =>
                {
                    mapCfg.FluentMappings.Add<EventMap>();
                    var assembly = Assembly.GetExecutingAssembly();
                    var eventSubclasses = assembly.GetTypes()
                        .Where(t => t.IsSubclassOf(typeof(Event)) && !t.IsAbstract);

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
                .UseMongoStorage(dbOptions.ConnectionString, dbOptions.Database, storageOptions);
        });
        Container.Initialize();

        _npgsqlClient = new NpgsqlConnection(dbPostgresConnectionString);
        await _npgsqlClient.OpenAsync();
        _hangfireServer = new BackgroundJobServer(new BackgroundJobServerOptions()
        {
            Queues = new[] { "events", "default" },
        });

        async Task pullPostgresImage()
        {
            await _docker.Images.CreateImageAsync(new ImagesCreateParameters
            {
                FromImage = "postgres",
                Tag = "latest"
            }, null, new Progress<JSONMessage>(message => { output.WriteLine(message.Status); }));
        }

        async Task pullMongoImage()
        {
            await _docker.Images.CreateImageAsync(new ImagesCreateParameters
            {
                FromImage = "mongo",
                Tag = "latest"
            }, null, new Progress<JSONMessage>(message => { output.WriteLine(message.Status); }));
        }

        async Task runPostgresContainer()
        {
            var container = await _docker.Containers.CreateContainerAsync(new CreateContainerParameters()
            {
                Image = "postgres",
                HostConfig = new HostConfig
                {
                    PortBindings = new Dictionary<string, IList<PortBinding>>
                    {
                        {"5432/tcp", [new PortBinding {HostPort = "5432"}]},
                    }
                },
                Env =
                [
                    "POSTGRES_PASSWORD=secret",
                    "POSTGRES_USER=iokode",
                    "POSTGRES_DB=testdb"
                ],
                Name = "oftest_nhibernate_postgres"
            });

            _postgresContainerId = container.ID;
            await _docker.Containers.StartContainerAsync(_postgresContainerId, new ContainerStartParameters());
        }

        async Task runMongoContainer()
        {
            var container = await _docker.Containers.CreateContainerAsync(new CreateContainerParameters
            {
                Image = "mongo",
                HostConfig = new HostConfig
                {
                    PortBindings = new Dictionary<string, IList<PortBinding>>
                    {
                        {"27017/tcp", new[] {new PortBinding {HostPort = "27017"}}},
                    }
                },
                Name = "oftest_hangfire_mongodb",
                Env = new List<string>
                {
                    "MONGO_INITDB_ROOT_USERNAME=root",
                    "MONGO_INITDB_ROOT_PASSWORD=Secret_123"
                },
            });

            _mongoContainerId = container.ID;
            await _docker.Containers.StartContainerAsync(_mongoContainerId, new ContainerStartParameters());
        }

        async Task waitUntilPostgresServerIsReady()
        {
            bool postgresServerIsReady = await PollingUtility.WaitUntilTrueAsync(async () =>
            {
                var containerInspect = await _docker.Containers.InspectContainerAsync(_postgresContainerId);
                bool containerIsReady = containerInspect.State.Running;
                if (!containerIsReady)
                {
                    return false;
                }

                try
                {
                    var client = new NpgsqlConnection(dbPostgresConnectionString);
                    await client.OpenAsync();
                    await client.QuerySingleAsync<int>("SELECT 1");
                    await client.CloseAsync();

                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }, timeout: 60_000, pollingInterval: 1_000);

            if (!postgresServerIsReady)
            {
                output.WriteLine("Failed to start Postgres server within the allowed time (30s).");
            }
        }

        async Task waitUntilMongoServerIsReady()
        {
            bool serverIsReady = await PollingUtility.WaitUntilTrueAsync(async () =>
            {
                var containerInspect = await _docker.Containers.InspectContainerAsync(_mongoContainerId);
                bool containerIsReady = containerInspect.State.Running;
                if (!containerIsReady)
                {
                    return false;
                }

                try
                {
                    var client = new MongoClient(dbOptions.ConnectionString);
                    var database = client.GetDatabase(dbOptions.Database);
                    var collection = database.GetCollection<BsonDocument>("InitializeTestCollection");

                    await collection.EstimatedDocumentCountAsync();

                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }, timeout: 30_000, pollingInterval: 1_000);

            if (!serverIsReady)
            {
                output.WriteLine("Failed to start MongoDB server within the allowed time (30s).");
            }
        }
    }

    public async Task DisposeAsync()
    {
        _hangfireServer.SendStop();
        await _hangfireServer.WaitForShutdownAsync(default);
        _hangfireServer.Dispose();

        await _npgsqlClient.CloseAsync();
        await _docker.Containers.StopContainerAsync(_postgresContainerId, new ContainerStopParameters());
        await _docker.Containers.RemoveContainerAsync(_postgresContainerId, new ContainerRemoveParameters());
        await _docker.Containers.StopContainerAsync(_mongoContainerId, new ContainerStopParameters());
        await _docker.Containers.RemoveContainerAsync(_mongoContainerId, new ContainerRemoveParameters());
        _docker.Dispose();
    }
    
    protected async Task CreateEventsTableQueryAsync()
    {
        await _npgsqlClient.ExecuteAsync(
            """
            CREATE TABLE Events (
                id TEXT PRIMARY KEY,
                event_type TEXT NOT NULL,
                dispatched_at TIMESTAMP,
                payload JSONB NOT NULL
            );
            """);
    }

    protected async Task DropEventsTableQueryAsync()
    {
        await _npgsqlClient.ExecuteAsync("DROP TABLE Events;");
    }
}