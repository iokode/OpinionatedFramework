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
/// Starts RabbitMQ and bootstraps the framework with the broker-backed event driver selected in configuration.
/// </summary>
/// <remarks>
/// The bus starts through the host, as it does in an application, so nothing here starts it by hand.
/// </remarks>
/// <remarks>
/// The handlers declared here are what create their queues in the broker. A process declaring none would still
/// be able to dispatch, which is the difference between this driver and the other two.
/// </remarks>
public class EventsTestsFixture : IAsyncLifetime
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
                events.AddEventHandler<OrderSubmitted, SendConfirmationEmail>();
                events.AddEventHandler<OrderSubmitted, UpdateStatistics>();
                events.AddEventHandler<OrderCancelled, FailOnFirstDelivery>(policy => policy.Retry(1));

                // Reacts to an event this application never raises.
                events.AddEventHandler<PartnerPayment, HandlePartnerPayment>();

                // Registered against the event interface, so a concrete event no handler names directly has to
                // be declared: a queue cannot be bound to an interface the transport could not construct.
                events.AddEvent<InventoryAdjusted>();
                events.AddEventHandler<ISubscribableEvent, StoreEvent>();
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

[CollectionDefinition(nameof(EventsTestsFixtureCollection))]
public class EventsTestsFixtureCollection : ICollectionFixture<EventsTestsFixture>;
