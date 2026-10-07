using System.Collections.Generic;
using MyServiceBus;

namespace IOKode.OpinionatedFramework.ContractImplementations.MyServiceBus;

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
