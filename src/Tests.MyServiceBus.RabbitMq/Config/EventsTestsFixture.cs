using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Bootstrapping;
using IOKode.OpinionatedFramework.ContractImplementations.MyServiceBus;
using IOKode.OpinionatedFramework.ServiceContainer;
using IOKode.OpinionatedFramework.TestHelpers.Containers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyServiceBus;
using Xunit;

namespace IOKode.OpinionatedFramework.Tests.MyServiceBus.RabbitMq.Config;

/// <summary>
/// Starts RabbitMQ and bootstraps the framework with the broker-backed event driver selected in configuration,
/// alongside another application that declares its own types for two of the same events.
/// </summary>
/// <remarks>
/// The bus starts through the host, as it does in an application, so nothing here starts it by hand.
/// </remarks>
/// <remarks>
/// The handlers declared here are what create their queues in the broker. A process declaring no handler at
/// all would still be able to raise what it declares it raises, which is the difference between this driver and
/// the other two.
/// </remarks>
/// <remarks>
/// The other application is a MyServiceBus bus of its own, built on its own service collection, so nothing it
/// registers can reach the container of the application under test. It is started first, because a broker
/// drops a published event that no queue is bound to yet.
/// </remarks>
public class EventsTestsFixture : IAsyncLifetime
{
    private readonly RabbitMqContainer rabbitMq = new();
    private HostHandle? host;
    private ServiceProvider? partnerApplication;
    private IMessageBus? partnerBus;

    /// <summary>Gets the bus of the other application, which the test raising its event publishes through.</summary>
    /// <exception cref="InvalidOperationException">The other application is not running.</exception>
    public IMessageBus PartnerBus => this.partnerBus
                                     ?? throw new InvalidOperationException("The other application is not running.");

    public async Task InitializeAsync()
    {
        await Container.Advanced.ResetAsync();
        HandledEvents.Reset();
        PartnerApplication.PartnerSubscriber.Reset();

        await this.rabbitMq.InitializeAsync();
        await this.StartPartnerApplicationAsync();

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
                events.Publishes<OrderSubmitted>();
                events.Publishes<OrderCancelled>();
                events.Publishes<InventoryAdjusted>();

                // Raised here and reacted to nowhere, which only the publish declaration reveals.
                events.Publishes<AuditRecorded>();

                events.Handles<OrderSubmitted, SendConfirmationEmail>();
                events.Handles<OrderSubmitted, UpdateStatistics>();
                events.Handles<OrderCancelled, FailOnFirstDelivery>(policy => policy.Retry(1));

                // Reacts to an event this application never raises.
                events.Handles<PartnerPayment, HandlePartnerPayment>();

                // Written against the event interface, so it is declared once per event it covers: a queue
                // carries one concrete event, and the transport has to construct what it received into it.
                events.Handles<OrderSubmitted, StoreEvent>();
                events.Handles<OrderCancelled, StoreEvent>();
                events.Handles<PartnerPayment, StoreEvent>();
                events.Handles<InventoryAdjusted, StoreEvent>();
            }));
    }

    public async Task DisposeAsync()
    {
        if (this.host is not null)
        {
            await this.host.DisposeAsync();
        }

        await Container.Advanced.ResetAsync();

        if (this.partnerBus is not null)
        {
            await this.partnerBus.StopAsync(CancellationToken.None);
        }

        if (this.partnerApplication is not null)
        {
            await this.partnerApplication.DisposeAsync();
        }

        await this.rabbitMq.DisposeAsync();
    }

    /// <summary>
    /// Starts the other application, outside the framework and outside its container.
    /// </summary>
    /// <remarks>
    /// Nothing of the framework reaches it, so it names the wire identity and the exchange of each contract
    /// itself. Those are the two names the driver derives from <c>[EventName]</c>, and agreeing on them is all
    /// it takes for two applications that share no type to exchange an event.
    /// </remarks>
    private async Task StartPartnerApplicationAsync()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddServiceBus(bus =>
        {
            bus.SetMessageUrn<PartnerApplication.PartnerPayment>("urn:message:tests.partner-payment");
            bus.SetMessageUrn<PartnerApplication.OrderSubmitted>("urn:message:tests.order-submitted");

            bus.AddConsumer<PartnerApplication.PartnerOrderConsumer, PartnerApplication.OrderSubmitted>(
                PartnerApplication.PartnerSubscriber.EndpointName);

            bus.UsingRabbitMq((context, rabbit) =>
            {
                rabbit.Message<PartnerApplication.PartnerPayment>(
                    message => message.SetEntityName("tests.partner-payment"));
                rabbit.Message<PartnerApplication.OrderSubmitted>(
                    message => message.SetEntityName("tests.order-submitted"));

                rabbit.Host("localhost", int.Parse(this.rabbitMq.Options.HostPort), host =>
                {
                    host.Username(this.rabbitMq.Options.Username);
                    host.Password(this.rabbitMq.Options.Password);
                });

                rabbit.ReceiveEndpoint(PartnerApplication.PartnerSubscriber.EndpointName,
                    endpoint => endpoint.ConfigureConsumer<PartnerApplication.PartnerOrderConsumer>(context));
            });
        });

        this.partnerApplication = services.BuildServiceProvider();

        this.partnerBus = this.partnerApplication.GetRequiredService<IMessageBus>();
        await this.partnerBus.StartAsync(CancellationToken.None);
    }
}

[CollectionDefinition(nameof(EventsTestsFixtureCollection))]
public class EventsTestsFixtureCollection : ICollectionFixture<EventsTestsFixture>;
