using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IOKode.OpinionatedFramework.Events;
using IOKode.OpinionatedFramework.Internals.Events;
using IOKode.OpinionatedFramework.ServiceContainer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MyServiceBus;

namespace IOKode.OpinionatedFramework.ContractImplementations.MyServiceBus;

/// <summary>
/// One handler subscribed to one concrete event type.
/// </summary>
/// <remarks>
/// Computed here rather than by each transport, because deciding which queue carries which event to which
/// handler is the same problem whichever broker is underneath.
/// </remarks>
/// <param name="EndpointName">The queue carrying this event to this handler.</param>
/// <param name="EventType">The concrete event type delivered to the consumer.</param>
/// <param name="HandlerType">The handler type.</param>
/// <param name="ConsumerType">The closed consumer type adapting the handler to the transport.</param>
/// <param name="Policy">How the transport is asked to execute the handler.</param>
public sealed record EventSubscription(
    string EndpointName,
    Type EventType,
    Type HandlerType,
    Type ConsumerType,
    MyServiceBusEventHandlerPolicy Policy);

/// <summary>
/// Configures the transport that carries the subscribed events.
/// </summary>
/// <remarks>
/// The subscriptions are supplied because a transport materializes them into its own topology, and applies the
/// part of the policy that its own configuration owns.
/// </remarks>
/// <param name="configurator">The bus registration being built.</param>
/// <param name="subscriptions">The handlers this process subscribes.</param>
public delegate void MyServiceBusTransportConfigurator(
    IBusRegistrationConfigurator configurator,
    IReadOnlyList<EventSubscription> subscriptions);

public static class MyServiceBusEventsServiceExtensions
{
    private static readonly MethodInfo addConsumerMethod = typeof(IRegistrationConfigurator).GetMethods()
        .Single(method => method is {Name: "AddConsumer", IsGenericMethodDefinition: true}
                          && method.GetGenericArguments().Length == 2
                          && method.GetParameters().Length == 2
                          && method.GetParameters()[0].ParameterType == typeof(string));

    private static readonly MethodInfo retryPipeMethod = typeof(MyServiceBusEventsServiceExtensions)
        .GetMethod(nameof(RetryPipe), BindingFlags.Static | BindingFlags.NonPublic)!;

    /// <summary>
    /// Registers the broker-backed event dispatcher and subscribes the handlers declared for it.
    /// </summary>
    /// <remarks>
    /// Everything that does not depend on the broker happens here: the wire identity of each event, which queue
    /// carries which event to which handler, the retry policy, and the dispatcher itself. A transport package
    /// supplies <paramref name="configureTransport"/>, which connects to the broker and materializes the queues.
    /// </remarks>
    /// <remarks>
    /// Each handler gets its own endpoint, named after the event and the handler, so subscribers are added and
    /// removed without touching each other or the publisher.
    /// </remarks>
    /// <param name="services">The framework service collection.</param>
    /// <param name="configureTransport">Connects the bus to a broker and materializes the subscriptions.</param>
    /// <param name="configuration">
    /// Declares the events and their handlers, or <see langword="null"/> to declare none.
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> or <paramref name="configureTransport"/> is <see langword="null"/>.</exception>
    /// <exception cref="DuplicateEventNameException">Two declared event types share a name.</exception>
    /// <exception cref="DuplicateEventEndpointException">Two handlers would subscribe to the same queue.</exception>
    public static void AddMyServiceBusEventDispatcher(this IOpinionatedServiceCollection services,
        MyServiceBusTransportConfigurator configureTransport,
        Action<MyServiceBusEventsOptions>? configuration = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureTransport);

        var options = new MyServiceBusEventsOptions();
        configuration?.Invoke(options);

        var declaredEventTypes = options.DeclaredEventTypes;
        EventNameUniqueness.EnsureUnique(declaredEventTypes);

        var subscriptions = BuildSubscriptions(options);

        services.AddServiceBus(configurator =>
        {
            foreach (var eventType in declaredEventTypes)
            {
                // The declared name, and not the CLR type, is what goes on the wire, so an event raised by
                // another application is recognized here even though the two declare their own types for it.
                // Every declared event is named, the ones this process raises as much as the ones it reacts
                // to: an event is written under its identity and read back by it.
                configurator.SetMessageUrn(eventType, EventMessageUrn.For(eventType));
            }

            foreach (var subscription in subscriptions)
            {
                // The endpoint name and the retry policy are core concepts, so they are declared here and not
                // left to the transport. Its concurrency limit is not: no registration overload accepts an
                // endpoint name, a concurrency limit and a pipe together, so the transport applies that one.
                var retryPipe = retryPipeMethod
                    .MakeGenericMethod(subscription.EventType)
                    .Invoke(null, [subscription.Policy]);

                addConsumerMethod
                    .MakeGenericMethod(subscription.ConsumerType, subscription.EventType)
                    .Invoke(configurator, [subscription.EndpointName, retryPipe]);
            }

            configureTransport(configurator, subscriptions);
        });

        foreach (var registration in options.EventHandlerRegistrations)
        {
            services.TryAddTransient(registration.HandlerType);
        }

        services.AddSingleton(options);
        services.AddTransient<IEventDispatcher, MyServiceBusEventDispatcher>();
    }

    /// <summary>
    /// Builds the consume pipe that applies the declared retry policy, or none when the policy declares no retry.
    /// </summary>
    private static Action<PipeConfigurator<ConsumeContext<TMessage>>>? RetryPipe<TMessage>(
        MyServiceBusEventHandlerPolicy policy)
        where TMessage : class
    {
        if (!policy.IsRetryConfigured)
        {
            return null;
        }

        return pipe => pipe.UseMessageRetry(retry =>
        {
            if (policy.RetryDelay is { } delay)
            {
                retry.Interval(policy.RetryCount, delay);
            }
            else
            {
                retry.Immediate(policy.RetryCount);
            }
        });
    }

    /// <summary>
    /// Turns every declared handler into the subscription that carries its event to it.
    /// </summary>
    /// <remarks>
    /// One declaration is one subscription: a handler is declared for the concrete event it reacts to, which is
    /// also the only thing a queue can be bound to and the only thing the transport can construct what it
    /// received into.
    /// </remarks>
    private static List<EventSubscription> BuildSubscriptions(MyServiceBusEventsOptions options)
    {
        var subscriptions = new List<EventSubscription>();
        var takenEndpoints = new Dictionary<string, Type>(StringComparer.Ordinal);

        foreach (var registration in options.EventHandlerRegistrations)
        {
            var endpointName = EventEndpointName.For(registration.EventType, registration.HandlerType);
            if (takenEndpoints.TryGetValue(endpointName, out var existingHandler))
            {
                throw new DuplicateEventEndpointException(
                    endpointName, registration.HandlerType, existingHandler);
            }

            takenEndpoints.Add(endpointName, registration.HandlerType);

            // Closes the consumer over the delivered event and the handler itself, so no part of the call has
            // to be reconstructed while a message is handled.
            var consumerType = typeof(EventHandlerConsumer<,>)
                .MakeGenericType(registration.EventType, registration.HandlerType);

            subscriptions.Add(new EventSubscription(
                endpointName,
                registration.EventType,
                registration.HandlerType,
                consumerType,
                registration.Policy));
        }

        return subscriptions;
    }
}
