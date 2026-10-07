using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Events.Exceptions;
using IOKode.OpinionatedFramework.ServiceLocation;

namespace IOKode.OpinionatedFramework.Events.Extensions;

/// <summary>
/// Raises an event without the application having to resolve a dispatcher itself.
/// </summary>
public static class EventExtensions
{
    /// <summary>
    /// Dispatches the event through the event dispatcher of the selected driver.
    /// </summary>
    /// <remarks>
    /// The dispatcher is resolved from the container, so the work goes through the contract rather than around
    /// it, and what the completed task promises is what <see cref="IEventDispatcher.DispatchAsync"/> promises.
    /// </remarks>
    /// <param name="event">The event to raise.</param>
    /// <param name="cancellationToken">
    /// Cancels the dispatch, not the execution of the handlers.
    /// </param>
    /// <exception cref="MissingPublishDeclarationException">
    /// The concrete type of <paramref name="event"/> was not declared with
    /// <see cref="EventDeclarations{TPolicy}.Publishes{TEvent}"/>.
    /// </exception>
    public static async Task DispatchAsync(this IPublishableEvent @event, CancellationToken cancellationToken = default)
    {
        var dispatcher = Locator.Resolve<IEventDispatcher>();
        await dispatcher.DispatchAsync(@event, cancellationToken);
    }
}
