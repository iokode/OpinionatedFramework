using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Events;
using MyServiceBus;

namespace IOKode.OpinionatedFramework.ContractImplementations.MyServiceBus;

/// <summary>
/// Adapts one framework event handler to a MyServiceBus consumer.
/// </summary>
/// <remarks>
/// One closed consumer type exists per subscribed handler, which gives every handler its own queue and its own
/// retry and concurrency settings. A handler that keeps failing therefore fills its own error queue without
/// affecting the other subscribers of the same event.
/// </remarks>
/// <remarks>
/// The message type and the declared type are separate parameters because they need not be the same: the
/// transport delivers a concrete event, while the handler may have been registered against an event interface.
/// Naming both keeps the consumer fully typed, so the transport resolves the handler and the call is direct.
/// </remarks>
/// <remarks>
/// A failure is allowed to leave this method so the receive pipeline sees it, applies the endpoint's retry
/// policy and, once exhausted, settles the delivery as failed.
/// </remarks>
/// <typeparam name="TMessage">The concrete event type delivered to this consumer.</typeparam>
/// <typeparam name="TDeclared">The event type the handler was registered against.</typeparam>
/// <typeparam name="THandler">The handler type this consumer runs.</typeparam>
public sealed class EventHandlerConsumer<TMessage, TDeclared, THandler>(THandler handler) : IConsumer<TMessage>
    where TDeclared : ISubscribableEvent
    where TMessage : class, TDeclared
    where THandler : class, IEventHandler<TDeclared>
{
    /// <inheritdoc/>
    public Task Consume(ConsumeContext<TMessage> context)
    {
        return handler.HandleAsync(context.Message, context.CancellationToken);
    }
}
