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
/// A handler is declared for the concrete event it reacts to, so the delivered event is the event the handler
/// is asked for and nothing has to be reconciled here. A handler written against a group of events satisfies
/// <see cref="IEventHandler{TEvent}"/> for the concrete one because that parameter is contravariant.
/// </remarks>
/// <remarks>
/// A failure is allowed to leave this method so the receive pipeline sees it, applies the endpoint's retry
/// policy and, once exhausted, settles the delivery as failed.
/// </remarks>
/// <typeparam name="TMessage">The concrete event type delivered to this consumer.</typeparam>
/// <typeparam name="THandler">The handler type this consumer runs.</typeparam>
public sealed class EventHandlerConsumer<TMessage, THandler>(THandler handler) : IConsumer<TMessage>
    where TMessage : class, ISubscribableEvent
    where THandler : class, IEventHandler<TMessage>
{
    /// <inheritdoc/>
    public Task Consume(ConsumeContext<TMessage> context)
    {
        return handler.HandleAsync(context.Message, context.CancellationToken);
    }
}
