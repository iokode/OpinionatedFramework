using System;
using System.Threading.Tasks;
using Docker.DotNet;
using Hangfire;
using Hangfire.PostgreSql;
using IOKode.OpinionatedFramework.Bootstrapping;
using IOKode.OpinionatedFramework.ContractImplementations.Hangfire.Jobs;
using IOKode.OpinionatedFramework.ContractImplementations.LoggerEmail;
using IOKode.OpinionatedFramework.Emailing;
using IOKode.OpinionatedFramework.Jobs;
using IOKode.OpinionatedFramework.Logging;
using IOKode.OpinionatedFramework.Tests.Helpers;
using IOKode.OpinionatedFramework.Tests.Helpers.Containers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace IOKode.OpinionatedFramework.Tests.Hangfire.Config;

public class JobsTestsFixture : IAsyncLifetime
{
    private DockerClient docker => DockerHelper.DockerClient;

    public readonly PostgresContainer PostgresContainer = new();
    public BackgroundJobServer? HangfireServer = null;

    public Func<ITestOutputHelper> TestOutputHelperFactory { get; set; }

    public async Task InitializeAsync()
    {
        await PostgresContainer.InitializeAsync();

        GlobalConfiguration.Configuration
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(cfgPostgres => cfgPostgres.UseNpgsqlConnection(PostgresHelper.ConnectionString));
        HangfireServer = new BackgroundJobServer();
        await Task.Delay(3000);

        Container.Services.AddTransient<IJobEnqueuer, HangfireJobEnqueuer>();
        Container.Services.AddTransient<ILogging>(_ => new XUnitLogging(TestOutputHelperFactory()));
        Container.Services.AddTransient<IEmailSender, LoggerEmailSender>();
        Container.Initialize();
    }

    public async Task DisposeAsync()
    {
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

[CollectionDefinition(nameof(JobsTestsFixtureCollection))]
public class JobsTestsFixtureCollection : ICollectionFixture<JobsTestsFixture>;