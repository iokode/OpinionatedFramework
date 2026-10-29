using System;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Events;
using IOKode.OpinionatedFramework.Internals.Events;
using IOKode.OpinionatedFramework.Logging;
using IOKode.OpinionatedFramework.ServiceLocation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyServiceBus;

namespace IOKode.OpinionatedFramework.ContractImplementations.MyServiceBus;

/// <summary>
/// Raises <see cref="EventHandlerExhausted"/> when a handler fails its last attempt.
/// </summary>
/// <remarks>
/// It sits in the pipe of every handler, outside the retry filter, whether or not a retry policy is declared. The
/// retry filter only lets a failure out once the attempts are exhausted, and without one the first failure is
/// already the last, so a failure that reaches this filter is an exhaustion in both cases.
/// </remarks>
/// <remarks>
/// A cancellation of the delivery is not an exhaustion, and here it is a failure like any other on its way out:
/// stopping the bus cancels the token of every delivery in flight, and the handler leaves with
/// <see cref="OperationCanceledException"/>. Only that exception with the token of the delivery cancelled is
/// let through without raising the event. A handler throwing it on its own, as a timeout does, has failed.
/// </remarks>
/// <remarks>
/// The failure is rethrown after the event is raised, so the delivery is still settled as failed and the message
/// still reaches the endpoint's error queue.
/// </remarks>
/// <typeparam name="TMessage">The concrete event type delivered to the handler.</typeparam>
/// <param name="handlerType">The handler type the pipe runs.</param>
/// <param name="attempts">How many attempts the handler's policy allows, the first one included.</param>
public sealed class EventHandlerExhaustionFilter<TMessage>(Type handlerType, int attempts)
    : IFilter<ConsumeContext<TMessage>>
    where TMessage : class, ISubscribableEvent
{
    /// <inheritdoc/>
    public async Task Send(ConsumeContext<TMessage> context, IPipe<ConsumeContext<TMessage>> next)
    {
        try
        {
            await next.Send(context);
        }
        catch (Exception exception) when (!IsCancellation(exception, context))
        {
            await RaiseAsync(context);
            throw;
        }
    }

    private static bool IsCancellation(Exception exception, ConsumeContext<TMessage> context) =>
        exception is OperationCanceledException && context.CancellationToken.IsCancellationRequested;

    private async Task RaiseAsync(ConsumeContext<TMessage> context)
    {
        var eventName = EventName.Of<TMessage>();
        var serviceProvider = Locator.ServiceProvider;
        var options = serviceProvider?.GetService<MyServiceBusEventsOptions>();

        if (serviceProvider is null || options is null || !EventHandlerExhaustion.IsRaisedFor(options, eventName))
        {
            return;
        }

        try
        {
            var exhausted = EventHandlerExhaustion.Describe(
                eventName,
                EventSerializer.Serialize(context.Message, typeof(TMessage)),
                handlerType.FullName!,
                attempts);

            await serviceProvider.GetRequiredService<IEventDispatcher>()
                .DispatchAsync(exhausted, context.CancellationToken);
        }
        catch (Exception exception)
        {
            // The failure of the handler is what the receive pipeline has to see, not this one.
            serviceProvider.GetService<ILogging>()?.FromCategory<EventHandlerExhaustionFilter<TMessage>>().LogError(
                exception,
                "The handler {Handler} exhausted its attempts on the event {Event}, and raising {Exhausted} failed.",
                handlerType.Name,
                eventName,
                EventName.Of<EventHandlerExhausted>());
        }
    }
}
