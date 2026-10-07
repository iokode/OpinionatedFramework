using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Docker.DotNet;
using Hangfire;
using Hangfire.PostgreSql;
using IOKode.OpinionatedFramework.Bootstrapping;
using IOKode.OpinionatedFramework.ContractImplementations.Hangfire;
using IOKode.OpinionatedFramework.ServiceContainer;
using IOKode.OpinionatedFramework.Logging;
using IOKode.OpinionatedFramework.TestHelpers;
using IOKode.OpinionatedFramework.TestHelpers.Containers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace IOKode.OpinionatedFramework.Tests.Hangfire.Config;

/// <summary>
/// Bootstraps the framework with the Hangfire drivers selected in configuration.
/// </summary>
/// <remarks>
/// The storage and the server options come from the bootstrap alone, so the fixture registers nothing about
/// Hangfire on the side. Two servers run, each with its own worker pool and queue, which is what the
/// <c>Servers</c> dictionary is for.
/// </remarks>
public class JobsTestsFixture : IAsyncLifetime
{
    private DockerClient docker => DockerHelper.DockerClient;

    /// <summary>
    /// The worker count of the <c>default</c> server, which configuration alone decides.
    /// </summary>
    public const int DefaultServerWorkerCount = 4;

    /// <summary>
    /// The worker count the named <c>ConfigureServer</c> delegate sets on the <c>events</c> server, overriding
    /// the 9 its configuration entry carries.
    /// </summary>
    public const int EventsServerWorkerCount = 3;

    public readonly PostgresContainer PostgresContainer = new();
    private HostHandle? hostHandle;

    public Func<ITestOutputHelper>? TestOutputHelperFactory { get; set; }

    public async Task InitializeAsync()
    {
        await PostgresContainer.InitializeAsync();

        Container.Services.AddTransient<ILogging>(_ => new XUnitLogging(TestOutputHelperFactory?.Invoke() ?? throw new NullReferenceException("TestOutputHelperFactory is null. Did you forget to set it in the constructor?")));

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OpinionatedFramework:JobEnqueuer:Driver"] = "Hangfire",
                ["OpinionatedFramework:JobScheduler:Driver"] = "Hangfire",
                ["OpinionatedFramework:Events:Driver"] = "Hangfire",
                ["OpinionatedFramework:Events:Queue"] = "events",
                ["Hangfire:Servers:default:Queues:0"] = "default",
                ["Hangfire:Servers:default:WorkerCount"] = DefaultServerWorkerCount.ToString(),
                ["Hangfire:Servers:default:ShutdownTimeout"] = "00:00:30",
                ["Hangfire:Servers:events:Queues:0"] = "events",
                ["Hangfire:Servers:events:WorkerCount"] = "9",
                ["Hangfire:Servers:events:ShutdownTimeout"] = "00:00:30"
            })
            .Build();

        hostHandle = await OpinionatedFrameworkBootstrapping.StartAsync(configuration, options =>
            options.Hangfire(hangfire =>
            {
                hangfire.ConfigureHangfire(hangfireConfiguration => hangfireConfiguration
                    .UseRecommendedSerializerSettings()
                    .UsePostgreSqlStorage(postgres => postgres.UseNpgsqlConnection(PostgresHelper.ConnectionString)));

                // Only the events server is overridden, so the running servers show that a named delegate
                // reaches its own server, wins over the configured value, and leaves the other server alone.
                hangfire.ConfigureServer("events", server => server.WorkerCount = EventsServerWorkerCount);
            }).HangfireEvents(events =>
            {
                events.Publishes<OrderSubmitted>();
                events.Publishes<OrderCancelled>();

                // Raised here and reacted to nowhere, which only the publish declaration reveals.
                events.Publishes<AuditRecorded>();

                events.Handles<OrderSubmitted, SendConfirmationEmail>();
                events.Handles<OrderSubmitted, UpdateStatistics>();
                events.Handles<OrderSubmitted, FailOnFirstAttempt>(policy =>
                    policy.Retry(1, TimeSpan.FromSeconds(1)));
                events.Handles<OrderCancelled, AlwaysFail>(policy =>
                    policy.Retry(2, TimeSpan.FromSeconds(1)));
            }));

        await Task.Delay(3000);
    }

    public async Task DisposeAsync()
    {
        if (hostHandle != null)
        {
            await hostHandle.DisposeAsync();
        }

        await PostgresContainer.DisposeAsync();
        docker.Dispose();
    }
}

[CollectionDefinition(nameof(JobsTestsFixtureCollection))]
public class JobsTestsFixtureCollection : ICollectionFixture<JobsTestsFixture>;
