using System;

namespace IOKode.OpinionatedFramework.ContractImplementations.MyServiceBus.Exceptions;

/// <summary>
/// Thrown when two handlers would subscribe to the same queue.
/// </summary>
/// <remarks>
/// A queue is named after the event and the handler, so two handlers reaching the same name are two handlers
/// whose type names only differ in ways the name does not keep. Letting both subscribe would make one of them
/// steal the other's deliveries, so the registration fails instead.
/// </remarks>
/// <param name="endpointName">The queue both handlers would subscribe to.</param>
/// <param name="handlerType">The handler being registered.</param>
/// <param name="registeredHandlerType">The handler already registered for that queue.</param>
public sealed class DuplicateEventEndpointException(
    string endpointName, Type handlerType, Type registeredHandlerType)
    : Exception($"The handlers '{registeredHandlerType.FullName}' and '{handlerType.FullName}' would both " +
                $"subscribe to the queue '{endpointName}'. Rename one of them.")
{
    /// <summary>Gets the queue both handlers would subscribe to.</summary>
    public string EndpointName { get; } = endpointName;

    /// <summary>Gets the handler being registered.</summary>
    public Type HandlerType { get; } = handlerType;

    /// <summary>Gets the handler already registered for that queue.</summary>
    public Type RegisteredHandlerType { get; } = registeredHandlerType;
}
