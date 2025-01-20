using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using IOKode.OpinionatedFramework.Tests.Helpers.Configuration;
using IOKode.OpinionatedFramework.Utilities;
using MongoDB.Bson;
using MongoDB.Driver;
using Xunit.Abstractions;

namespace IOKode.OpinionatedFramework.Tests.Helpers.Containers;

public static class MongoHelper
{
    public static async Task<string> PullRunAndWaitMongoContainerAsync(DockerClient docker, MongoOptions mongoOptions, ITestOutputHelper? output = null)
    {
        await PullMongoImage(docker, output);
        var mongoContainerId = await RunMongoContainer(docker, output);
        await WaitUntilMongoServerIsReady(docker, mongoContainerId, mongoOptions, output);

        return mongoContainerId;
    }

    public static async Task WaitUntilMongoServerIsReady(DockerClient docker, string mongoContainerId, MongoOptions mongoOptions, ITestOutputHelper? output)
    {
        bool serverIsReady = await PollingUtility.WaitUntilTrueAsync(async () =>
        {
            output?.WriteLine("Waiting for MongoDB server to be ready...");
            var containerInspect = await docker.Containers.InspectContainerAsync(mongoContainerId);
            bool containerIsReady = containerInspect.State.Running;
            if (!containerIsReady)
            {
                output?.WriteLine("Not ready yet...");
                return false;
            }

            try
            {
                using var client = new MongoClient(mongoOptions.ConnectionString);
                var database = client.GetDatabase(mongoOptions.Database);
                var collection = database.GetCollection<BsonDocument>("InitializeTestCollection");

                await collection.EstimatedDocumentCountAsync();

                return true;
            }
            catch (Exception ex)
            {
                output?.WriteLine("Not ready yet...");
                output?.WriteLine(ex.Message);
                return false;
            }
        }, timeout: 30_000, pollingInterval: 1_000);

        if (!serverIsReady)
        {
            throw new TimeoutException("Failed to start Postgres server within the allowed time (30s).");
        }
    }

    public static async Task<string> RunMongoContainer(DockerClient docker, ITestOutputHelper? output)
    {
        var container = await docker.Containers.CreateContainerAsync(new CreateContainerParameters
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

        var mongoContainerId = container.ID;
        await docker.Containers.StartContainerAsync(mongoContainerId, new ContainerStartParameters());
        return mongoContainerId;
    }

    public static async Task PullMongoImage(DockerClient docker, ITestOutputHelper? output)
    {
        await docker.Images.CreateImageAsync(new ImagesCreateParameters
        {
            FromImage = "mongo",
            Tag = "latest"
        }, null, new Progress<JSONMessage>(message => { output?.WriteLine(message.Status); }));
    }

    public static readonly MongoOptions DefaultOptions = new()
    {
        Database = "IOKodeHangfireTests",
        ConnectionString = "mongodb://root:Secret_123@localhost:27017"
    };
    
    public static readonly MongoStorageOptions DefaultStorageOptions = new()
    {
        MigrationOptions = new MongoMigrationOptions
        {
            MigrationStrategy = new DropMongoMigrationStrategy()
        },
        CheckQueuedJobsStrategy = CheckQueuedJobsStrategy.TailNotificationsCollection
    };
}