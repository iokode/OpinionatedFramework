using IOKode.OpinionatedFramework.Events;

namespace IOKode.OpinionatedFramework.ContractImplementations.MyServiceBus;

/// <summary>
/// Declares the events this process raises and the handlers it subscribes to the broker.
/// </summary>
/// <remarks>
/// Unlike the in-memory driver, only what a process is itself responsible for is declared here. A publishing
/// process declares no handler, and a subscriber declares its own without the publisher knowing: the fan-out
/// happens at the broker rather than at the dispatcher.
/// </remarks>
/// <remarks>
/// The declared name of every event named here is what goes on the wire, which is why an event the process only
/// raises is declared too: the driver has to name it on the envelope and on the exchange it is published to.
/// </remarks>
public sealed class MyServiceBusEventsOptions : EventDeclarations<MyServiceBusEventHandlerPolicy>;
