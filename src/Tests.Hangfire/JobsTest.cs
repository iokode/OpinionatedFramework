using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cronos;
using Docker.DotNet;
using Docker.DotNet.Models;
using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using IOKode.OpinionatedFramework.ContractImplementations.Hangfire.Jobs;
using IOKode.OpinionatedFramework.Jobs;
using IOKode.OpinionatedFramework.Utilities;
using MongoDB.Bson;
using MongoDB.Driver;
using Xunit;
using Xunit.Abstractions;

namespace IOKode.OpinionatedFramework.Tests.Hangfire;

public class JobsTest : IAsyncLifetime
{
    public static bool isEnqueuedJobExecuted;
    public static bool isEnqueuedWithDelayJobExecuted;
    public static int scheduledJobExecutions;
    public static bool isRescheduledJobExecuted;
    public static bool isUnscheduleJobExecuted;

    private readonly ITestOutputHelper output;
    private string containerId = null!;
    private DockerClient docker = null!;
    private BackgroundJobServer hangfireServer = null!;

    public JobsTest(ITestOutputHelper output)
    {
        this.output = output;
    }

    public async Task InitializeAsync()
    {
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

        docker = new DockerClientConfiguration().CreateClient();
        await pullMongoImage();
        await runMongoContainer();
        await waitUntilMongoServerIsReady();

        GlobalConfiguration.Configuration
            .UseRecommendedSerializerSettings()
            .UseMongoStorage(dbOptions.ConnectionString, dbOptions.Database, storageOptions);
        hangfireServer = new BackgroundJobServer();

        async Task pullMongoImage()
        {
            await docker.Images.CreateImageAsync(new ImagesCreateParameters
            {
                FromImage = "mongo",
                Tag = "latest"
            }, null, new Progress<JSONMessage>(message => { output.WriteLine(message.Status); }));
        }

        async Task runMongoContainer()
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

            containerId = container.ID;
            await docker.Containers.StartContainerAsync(containerId, new ContainerStartParameters());
        }

        async Task waitUntilMongoServerIsReady()
        {
            bool serverIsReady = await PollingUtility.WaitUntilTrueAsync(async () =>
            {
                var containerInspect = await docker.Containers.InspectContainerAsync(containerId);
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
        hangfireServer.SendStop();
        await hangfireServer.WaitForShutdownAsync(default);
        hangfireServer.Dispose();

        await docker.Containers.StopContainerAsync(containerId, new ContainerStopParameters());
        await docker.Containers.RemoveContainerAsync(containerId, new ContainerRemoveParameters());
        docker.Dispose();
    }

    [Fact]
    public async Task EnqueueJob_Success()
    {
        // Arrange
        var jobEnqueuer = new HangfireJobEnqueuer();
        var job = new EnqueuedJob();

        // Act
        await jobEnqueuer.EnqueueAsync(Queue.Default, job, default);
        await Task.Delay(2000);

        // Assert
        Assert.True(isEnqueuedJobExecuted);
    }

    [Fact]
    public async Task EnqueueJobWithDelay_Success()
    {
        // Arrange
        var jobEnqueuer = new HangfireJobEnqueuer();
        var job = new EnqueuedWithDelayJob();

        // Act
        await jobEnqueuer.EnqueueWithDelayAsync(Queue.Default, job, TimeSpan.FromMilliseconds(5000), default);
        await Task.Delay(16000);

        // Assert
        Assert.True(isEnqueuedWithDelayJobExecuted);
    }

    [Fact]
    public async Task ScheduleJob_Success()
    {
        // Arrange
        var jobScheduler = new HangfireJobScheduler();
        var job = new ScheduledJob();

        // Act
        var scheduledJob = await jobScheduler.ScheduleAsync(job, CronExpression.Parse("0/15 * * * * *", CronFormat.IncludeSeconds), default);
        await Task.Delay(32000);
        await jobScheduler.UnscheduleAsync(scheduledJob, default);

        // Assert
        Assert.InRange(scheduledJobExecutions, 2, 3);
    }

    [Fact]
    public async Task RescheduleJob_Success()
    {
        // Arrange
        var jobScheduler = new HangfireJobScheduler();
        var job = new RescheduledJob();

        // Act
        var scheduledJob = await jobScheduler.ScheduleAsync(job, CronExpression.Parse("0/20 * * * *"), default);
        await Task.Delay(15000);
        await jobScheduler.RescheduleAsync(scheduledJob, CronExpression.Parse("0/15 * * * * *", CronFormat.IncludeSeconds), default);
        await Task.Delay(16000);
        await jobScheduler.UnscheduleAsync(scheduledJob, default);

        // Assert
        Assert.True(isRescheduledJobExecuted);
    }

    [Fact]
    public async Task UnscheduleJob_Success()
    {
        // Arrange
        var jobScheduler = new HangfireJobScheduler();
        var job = new UnscheduledJob();

        // Act
        var scheduledJob = await jobScheduler.ScheduleAsync(job, CronExpression.Parse("0 * * * *"), default);
        await jobScheduler.UnscheduleAsync(scheduledJob, default);
        await Task.Delay(5000);

        // Assert
        Assert.False(isUnscheduleJobExecuted);
    }

    public class EnqueuedJob : IJob
    {
        public Task InvokeAsync(CancellationToken cancellationToken)
        {
            isEnqueuedJobExecuted = true;
            return Task.CompletedTask;
        }
    }

    public class EnqueuedWithDelayJob : IJob
    {
        public Task InvokeAsync(CancellationToken cancellationToken)
        {
            isEnqueuedWithDelayJobExecuted = true;
            return Task.CompletedTask;
        }
    }

    public class ScheduledJob : IJob
    {
        public Task InvokeAsync(CancellationToken cancellationToken)
        {
            scheduledJobExecutions++;
            return Task.CompletedTask;
        }
    }

    public class RescheduledJob : IJob
    {
        public Task InvokeAsync(CancellationToken cancellationToken)
        {
            isRescheduledJobExecuted = true;
            return Task.CompletedTask;
        }
    }

    public class UnscheduledJob : IJob
    {
        public Task InvokeAsync(CancellationToken cancellationToken)
        {
            isUnscheduleJobExecuted = true;
            return Task.CompletedTask;
        }
    }
}