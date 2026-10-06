using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Bootstrapping;
using IOKode.OpinionatedFramework.ServiceContainer;
using IOKode.OpinionatedFramework.TestHelpers.Containers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyServiceBus;
using Xunit;

namespace IOKode.OpinionatedFramework.Tests.MyServiceBus.RabbitMq.Config;

/// <summary>
/// Records what the application outside the framework received.
/// </summary>
public static class ExternalSubscriber
{
    /// <summary>The queue of the application that reacts, which the framework knows nothing about.</summary>
    public const string EndpointName = "tests--external-audit-observer";

    private static readonly ConcurrentBag<Guid> received = [];

    public static void Record(Guid eventId) => received.Add(eventId);

    public static bool Received(Guid eventId) => received.Any(id => id == eventId);

    public static void Reset() => received.Clear();
}

/// <summary>
/// Reacts to the event without the framework taking part, standing in for the other application.
/// </summary>
/// <remarks>
/// A plain MyServiceBus consumer rather than an <see cref="Events.IEventHandler{TEvent}"/>, because the point
/// of the fixture is that the application under test declares no handler at all.
/// </remarks>
public class ExternalAuditConsumer : IConsumer<AuditRecorded>
{
    public Task Consume(ConsumeContext<AuditRecorded> context)
    {
        ExternalSubscriber.Record(context.Message.OrderId);
        return Task.CompletedTask;
    }
}

/// <summary>
/// Bootstraps the framework with the broker-backed event driver selected and not a single handler declared,
/// alongside a separate application that subscribes the event this one only emits.
/// </summary>
/// <remarks>
/// This is the shape of a process that emits and never reacts, which is the one thing the other fixture cannot
/// exercise: it always declares handlers, so the bus always has receive endpoints. Here the bus has none.
/// </remarks>
/// <remarks>
/// The subscribing application is a MyServiceBus bus of its own, built on its own service collection, so
/// nothing it registers can reach the container of the application under test. It is started first, because a
/// broker drops a published event that no queue is bound to yet.
/// </remarks>
public class PublisherOnlyFixture : IAsyncLifetime
{
    private readonly RabbitMqContainer rabbitMq = new();
    private HostHandle? host;
    private ServiceProvider? subscribingApplication;
    private IMessageBus? subscribingBus;

    public async Task InitializeAsync()
    {
        await Container.Advanced.ResetAsync();
        ExternalSubscriber.Reset();

        await this.rabbitMq.InitializeAsync();
        await this.StartSubscribingApplicationAsync();

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

        // The MyServiceBusEvents verb is not called at all: this application declares no handler and no event,
        // which is all a process that only emits has to do.
        this.host = await OpinionatedFrameworkBootstrapping.StartAsync(configuration);
    }

    public async Task DisposeAsync()
    {
        if (this.host is not null)
        {
            await this.host.DisposeAsync();
        }

        await Container.Advanced.ResetAsync();

        if (this.subscribingBus is not null)
        {
            await this.subscribingBus.StopAsync(CancellationToken.None);
        }

        if (this.subscribingApplication is not null)
        {
            await this.subscribingApplication.DisposeAsync();
        }

        await this.rabbitMq.DisposeAsync();
    }

    /// <summary>
    /// Starts the application that reacts to the event, outside the framework and outside its container.
    /// </summary>
    private async Task StartSubscribingApplicationAsync()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddServiceBus(bus =>
        {
            bus.AddConsumer<ExternalAuditConsumer, AuditRecorded>(ExternalSubscriber.EndpointName);
            bus.UsingRabbitMq((context, rabbit) =>
            {
                rabbit.Host("localhost", int.Parse(this.rabbitMq.Options.HostPort), host =>
                {
                    host.Username(this.rabbitMq.Options.Username);
                    host.Password(this.rabbitMq.Options.Password);
                });

                rabbit.ReceiveEndpoint(ExternalSubscriber.EndpointName,
                    endpoint => endpoint.ConfigureConsumer<ExternalAuditConsumer>(context));
            });
        });

        this.subscribingApplication = services.BuildServiceProvider();

        // Outside a host, the actions that bind the consumers to the topology have to be run by hand: the
        // MyServiceBus hosted service is what does it in an application, and there is no host here.
        foreach (var action in this.subscribingApplication.GetServices<IPostBuildAction>())
        {
            action.Execute(this.subscribingApplication);
        }

        this.subscribingBus = this.subscribingApplication.GetRequiredService<IMessageBus>();
        await this.subscribingBus.StartAsync(CancellationToken.None);
    }
}

[CollectionDefinition(nameof(PublisherOnlyFixtureCollection))]
public class PublisherOnlyFixtureCollection : ICollectionFixture<PublisherOnlyFixture>;
