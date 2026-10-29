using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.PostgreSql;
using IOKode.OpinionatedFramework.Bootstrapping;
using IOKode.OpinionatedFramework.ContractImplementations.Hangfire;
using IOKode.OpinionatedFramework.Events;
using IOKode.OpinionatedFramework.Logging;
using IOKode.OpinionatedFramework.ServiceContainer;
using IOKode.OpinionatedFramework.TestHelpers;
using IOKode.OpinionatedFramework.TestHelpers.Containers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace IOKode.OpinionatedFramework.Tests.Hangfire.Config;

/// <summary>
/// Bootstraps the framework with the Hangfire event driver selected, a handler that fails and a handler of
/// <see cref="EventHandlerExhausted"/>, without opting into the built-in events.
/// </summary>
/// <remarks>
/// This is the one shape the other fixture cannot exercise: it opts in, because most of what it tests needs the
/// exhaustion raised. It runs on a storage of its own, so no job of the other fixture reaches its server.
/// </remarks>
/// <remarks>
/// The container is reset on both ends, because the other fixture of this assembly leaves it built.
/// </remarks>
public class WithoutBuiltInEventsFixture : IAsyncLifetime
{
    private readonly PostgresContainer postgresContainer = new();
    private HostHandle? hostHandle;

    public Func<ITestOutputHelper>? TestOutputHelperFactory { get; set; }

    public async Task InitializeAsync()
    {
        await Container.Advanced.ResetAsync();
        HandledEvents.Reset();

        await this.postgresContainer.InitializeAsync();

        Container.Services.AddTransient<ILogging>(_ => new XUnitLogging(TestOutputHelperFactory?.Invoke() ?? throw new NullReferenceException("TestOutputHelperFactory is null. Did you forget to set it in the constructor?")));

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OpinionatedFramework:JobEnqueuer:Driver"] = "Hangfire",
                ["OpinionatedFramework:Events:Driver"] = "Hangfire",
                ["OpinionatedFramework:Events:Queue"] = "events",
                ["Hangfire:Servers:events:Queues:0"] = "events",
                ["Hangfire:Servers:events:WorkerCount"] = "2",
                ["Hangfire:Servers:events:ShutdownTimeout"] = "00:00:30"
            })
            .Build();

        this.hostHandle = await OpinionatedFrameworkBootstrapping.StartAsync(configuration, options =>
            options.Hangfire(hangfire => hangfire.ConfigureHangfire(hangfireConfiguration => hangfireConfiguration
                    .UseRecommendedSerializerSettings()
                    .UsePostgreSqlStorage(postgres => postgres.UseNpgsqlConnection(PostgresHelper.ConnectionString))))
                .HangfireEvents(events =>
                {
                    events.Publishes<OrderRejected>();

                    // Reacting to the event is declared; raising it is not.
                    events.Handles<OrderRejected, AlwaysFailWithoutRetry>(policy => policy.Retry(0));
                    events.Handles<EventHandlerExhausted, RecordExhaustion>(policy => policy.Retry(0));
                }));
    }

    public async Task DisposeAsync()
    {
        if (this.hostHandle != null)
        {
            await this.hostHandle.DisposeAsync();
        }

        await Container.Advanced.ResetAsync();
        await this.postgresContainer.DisposeAsync();
    }
}

[CollectionDefinition(nameof(WithoutBuiltInEventsFixtureCollection))]
public class WithoutBuiltInEventsFixtureCollection : ICollectionFixture<WithoutBuiltInEventsFixture>;
