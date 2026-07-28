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
/// Hangfire on the side. The worker serves the <c>events</c> queue as well as the default one, which is what the
/// configured queues are for.
/// </remarks>
public class JobsTestsFixture : IAsyncLifetime
{
    private DockerClient docker => DockerHelper.DockerClient;

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
                ["OpinionatedFramework:JobEnqueuer:Driver"] = "hangfire",
                ["OpinionatedFramework:JobScheduler:Driver"] = "hangfire",
                ["Hangfire:StartWorker"] = "true",
                ["Hangfire:Queues:0"] = "default",
                ["Hangfire:Queues:1"] = "events",
                ["Hangfire:WorkerCount"] = "4",
                ["Hangfire:ShutdownTimeout"] = "00:00:30"
            })
            .Build();

        hostHandle = await OpinionatedFrameworkBootstrapping.StartAsync(configuration, options =>
            options.Hangfire(hangfire => hangfire.ConfigureHangfire(hangfireConfiguration => hangfireConfiguration
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(postgres => postgres.UseNpgsqlConnection(PostgresHelper.ConnectionString)))));

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
