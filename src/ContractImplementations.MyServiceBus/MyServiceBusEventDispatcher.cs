using System;
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
public class MyServiceBusEventDispatcher(MyServiceBusEventsOptions options, IServiceProvider serviceProvider)
    : IEventDispatcher
{
    /// <inheritdoc/>
    public Task DispatchAsync(IPublishableEvent @event, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(@event);

        // The concrete type decides the exchange and the wire identity the event is published under, so a
        // variable typed as the base still reaches the handlers subscribed to the actual event. It is passed
        // explicitly because the overload that infers it would infer the declared type of the argument, which
        // here is the interface every publishable event implements.
        var eventType = @event.GetType();

        // An event the application never declared it raises has no identity on this bus, so it would be
        // published under its CLR type and reach nothing that expects it.
        options.EnsureDeclaredAsPublished(eventType);

        var publishEndpoint = serviceProvider.GetRequiredService<IPublishEndpoint>();

        return publishEndpoint.Publish(@event, eventType, cancellationToken: cancellationToken);
    }
}
