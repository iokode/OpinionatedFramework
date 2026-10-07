using System;
using System.Text;
using IOKode.OpinionatedFramework.Events;
using IOKode.OpinionatedFramework.Events.Exceptions;

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
