using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Events;
using Microsoft.Extensions.DependencyInjection;
using MyServiceBus;

namespace IOKode.OpinionatedFramework.ContractImplementations.MyServiceBus;

/// <summary>
/// Dispatches events by publishing them to the broker.
/// </summary>
/// <remarks>
/// The dispatcher does not know which handlers exist. It publishes once and the broker copies the event into
/// the queue of every subscribed handler, so a handler can live in another process, or another application,
/// without the publishing side being aware of it.
/// </remarks>
/// <remarks>
/// The event is durable once the broker has accepted it. Publishing happens outside any transaction the
/// application may have open, so a crash between committing a business change and publishing loses the event.
/// </remarks>
public class MyServiceBusEventDispatcher(IServiceProvider serviceProvider) : IEventDispatcher
{
    private static readonly MethodInfo publishMethod = typeof(IPublishEndpoint).GetMethods()
        .Single(method => method is {Name: nameof(IPublishEndpoint.Publish), IsGenericMethodDefinition: true}
                          && method.GetParameters()[0].ParameterType == typeof(object));

    private static readonly ConcurrentDictionary<Type, MethodInfo> publishByEventType = new();

    /// <inheritdoc/>
    public Task DispatchAsync(IPublishableEvent @event, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(@event);

        // The concrete type decides the exchange the event is published to, so a variable typed as the base
        // still reaches the handlers subscribed to the actual event.
        var eventType = @event.GetType();
        _ = EventName.Of(eventType);

        var publishEndpoint = serviceProvider.GetRequiredService<IPublishEndpoint>();
        var publish = publishByEventType.GetOrAdd(eventType, static type => publishMethod.MakeGenericMethod(type));

        return (Task) publish.Invoke(publishEndpoint, [@event, null, cancellationToken])!;
    }
}
