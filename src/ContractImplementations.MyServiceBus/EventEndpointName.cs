using System;
using System.Text;
using IOKode.OpinionatedFramework.Events;

namespace IOKode.OpinionatedFramework.ContractImplementations.MyServiceBus;

/// <summary>
/// Builds the queue name that carries one event to one handler.
/// </summary>
/// <remarks>
/// The name is derived from the declared event name and the handler type, never from the CLR event type, so a
/// queue keeps its identity across refactors and stays recognizable in the broker's management tools.
/// </remarks>
public static class EventEndpointName
{
    /// <summary>
    /// Builds the queue name for a handler subscribed to a concrete event type.
    /// </summary>
    /// <param name="eventType">The concrete event type.</param>
    /// <param name="handlerType">The handler type.</param>
    /// <exception cref="ArgumentNullException">Any argument is <see langword="null"/>.</exception>
    /// <exception cref="MissingEventNameException">The event type declares no name.</exception>
    public static string For(Type eventType, Type handlerType)
    {
        ArgumentNullException.ThrowIfNull(eventType);
        ArgumentNullException.ThrowIfNull(handlerType);

        return $"{EventName.Of(eventType)}--{ToKebabCase(handlerType.Name)}";
    }

    private static string ToKebabCase(string name)
    {
        var builder = new StringBuilder(name.Length + 8);
        for (var index = 0; index < name.Length; index++)
        {
            var character = name[index];
            if (char.IsUpper(character))
            {
                if (index > 0)
                {
                    builder.Append('-');
                }

                builder.Append(char.ToLowerInvariant(character));
                continue;
            }

            builder.Append(character);
        }

        return builder.ToString();
    }
}

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
