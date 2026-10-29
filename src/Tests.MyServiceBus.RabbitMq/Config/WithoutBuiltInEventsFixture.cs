using System.Collections.Generic;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Bootstrapping;
using IOKode.OpinionatedFramework.ContractImplementations.MyServiceBus;
using IOKode.OpinionatedFramework.Events;
using IOKode.OpinionatedFramework.ServiceContainer;
using IOKode.OpinionatedFramework.TestHelpers.Containers;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace IOKode.OpinionatedFramework.Tests.MyServiceBus.RabbitMq.Config;

/// <summary>
/// Bootstraps the framework with the broker-backed event driver selected, a handler that fails and a handler of
/// <see cref="EventHandlerExhausted"/>, without opting into the built-in events.
/// </summary>
/// <remarks>
/// This is the one shape the other fixture cannot exercise: it opts in, because most of what it tests needs the
/// exhaustion raised.
/// </remarks>
public class WithoutBuiltInEventsFixture : IAsyncLifetime
{
    private readonly RabbitMqContainer rabbitMq = new();
    private HostHandle? host;

    public async Task InitializeAsync()
    {
        await Container.Advanced.ResetAsync();
        HandledEvents.Reset();

        await this.rabbitMq.InitializeAsync();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OpinionatedFramework:Events:Driver"] = "MyServiceBus.RabbitMq",
                ["OpinionatedFramework:Events:Host"] = "localhost",
                ["OpinionatedFramework:Events:Port"] = this.rabbitMq.Options.HostPort,
                ["OpinionatedFramework:Events:Username"] = this.rabbitMq.Options.Username,
                ["OpinionatedFramework:Events:Password"] = this.rabbitMq.Options.Password
            })
            .Build();

        this.host = await OpinionatedFrameworkBootstrapping.StartAsync(configuration, options =>
            options.MyServiceBusEvents(events =>
            {
                events.Publishes<OrderRejected>();

                // Reacting to the event is declared; raising it is not.
                events.Handles<OrderRejected, AlwaysFailWithoutRetry>();
                events.Handles<EventHandlerExhausted, RecordExhaustion>();
            }));
    }

    public async Task DisposeAsync()
    {
        if (this.host is not null)
        {
            await this.host.DisposeAsync();
        }

        await Container.Advanced.ResetAsync();
        await this.rabbitMq.DisposeAsync();
    }
}

[CollectionDefinition(nameof(WithoutBuiltInEventsFixtureCollection))]
public class WithoutBuiltInEventsFixtureCollection : ICollectionFixture<WithoutBuiltInEventsFixture>;
