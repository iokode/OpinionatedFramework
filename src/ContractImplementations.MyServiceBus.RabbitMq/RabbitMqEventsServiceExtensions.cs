using System;
using System.Linq;
using System.Reflection;
using IOKode.OpinionatedFramework.ContractImplementations.MyServiceBus;
using IOKode.OpinionatedFramework.ServiceContainer;
using MyServiceBus;

namespace IOKode.OpinionatedFramework.ContractImplementations.MyServiceBus.RabbitMq;

public static class RabbitMqEventsServiceExtensions
{
    private static readonly MethodInfo configureConsumerMethod = typeof(ReceiveEndpointConfigurator)
        .GetMethods()
        .Single(method => method is {Name: nameof(ReceiveEndpointConfigurator.ConfigureConsumer),
                              IsGenericMethodDefinition: true});

    /// <summary>
    /// Registers the event dispatcher carried by RabbitMQ.
    /// </summary>
    /// <remarks>
    /// The subscriptions and their retry policy are decided by the transport-neutral package. What belongs here
    /// is the connection, the receive endpoint each subscription is materialized into, and the concurrency
    /// limit, which no transport-neutral registration overload can express together with an endpoint name and a
    /// pipe.
    /// </remarks>
    /// <param name="services">The framework service collection.</param>
    /// <param name="connection">The broker connection.</param>
    /// <param name="configuration">Declares the handlers, or <see langword="null"/> to subscribe none.</param>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> or <paramref name="connection"/> is <see langword="null"/>.</exception>
    /// <exception cref="DuplicateEventNameException">Two declared event types share a name.</exception>
    /// <exception cref="DuplicateEventEndpointException">Two handlers would subscribe to the same queue.</exception>
    public static void AddMyServiceBusRabbitMqEventDispatcher(this IOpinionatedServiceCollection services,
        MyServiceBusConnection connection, Action<MyServiceBusEventsOptions>? configuration = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(connection);

        services.AddMyServiceBusEventDispatcher(
            (configurator, subscriptions) => configurator.UsingRabbitMq((context, rabbit) =>
            {
                rabbit.Host(connection.Host, connection.Port, host =>
                {
                    host.Username(connection.Username);
                    host.Password(connection.Password);
                });

                foreach (var subscription in subscriptions)
                {
                    rabbit.ReceiveEndpoint(subscription.EndpointName, endpoint =>
                    {
                        if (subscription.Policy.ConcurrencyLimit is { } limit)
                        {
                            endpoint.ConcurrentMessageLimit(limit);
                        }

                        configureConsumerMethod
                            .MakeGenericMethod(subscription.ConsumerType)
                            .Invoke(endpoint, [context]);
                    });
                }
            }),
            configuration);
    }
}
